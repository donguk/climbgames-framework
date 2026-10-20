using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClimbGames.Editor.UI
{
    public class PrefabWindow : EditorWindow
    {
        private const string WindowUxmlGUID = "8871c4d2fda088847a0c7fd3ac3c627e";
        private const string PrefabCellUxmlGUID = "5da4454cd02d87443a922c64360d0ae8";
        private const string BaseTypeCellUxmlGUID = "67648f2031ce73a46997d10736f7cc65";

        private FileTreeView treeView;
        private Dictionary<string, PrefabConvertData> convertDatas = new Dictionary<string, PrefabConvertData>();
        private FileMenuItem[] prefabCellMenuItems;

        [MenuItem("Tools/ClimbGames/UI Prefab Convert")]
        private static void Open()
        {
            var window = GetWindow<PrefabWindow>();
            window.titleContent = new GUIContent("UI Prefab Convert");
            window.minSize = new Vector2(400, 300);
        }

        void OnDisable()
        {
            treeView?.ExpansionState.Save("PrefabWindow");
        }

        void CreateGUI()
        {
            string windowUxmlPath = AssetDatabase.GUIDToAssetPath(WindowUxmlGUID);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowUxmlPath);
            visualTree.CloneTree(rootVisualElement);

            var rawPathField = rootVisualElement.Q<SelectPathField>("raw_path");
            rawPathField.Path = PrefabConvertSettings.OriginPath;
            rawPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                PrefabConvertSettings.OriginPath = relativePath;
                SetOriginPath(relativePath);

                treeView.ExpansionState.Clear("PrefabWindow");
                return relativePath;
            };

            var outputPathField = rootVisualElement.Q<SelectPathField>("output_path");
            outputPathField.Path = PrefabConvertSettings.OutputPath;
            outputPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                PrefabConvertSettings.OutputPath = relativePath;
                RefreshConvertDatas();
                return relativePath;
            };

            var codeGenPathField = rootVisualElement.Q<SelectPathField>("codegen_path");
            codeGenPathField.Path = PrefabConvertSettings.ScriptPath;
            codeGenPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                return PrefabConvertSettings.ScriptPath = relativePath;
            };

            // 프리팹 뷰
            treeView = rootVisualElement.Q<FileTreeView>("prefab_view");

            treeView.AddFileMenuItem(new FileMenuItem[]
            {
                new FileMenuItem("Convert Prefab", FileMenuFlag.File),
            });
            treeView.onContextMenuClicked += OnContextMenuClicked;
            treeView.onFileMouseDownEvent += OnFileMouseDownEvent;

            prefabCellMenuItems = new FileMenuItem[]
            {
                new FileMenuItem("Refresh", FileMenuFlag.File),
                new FileMenuItem("Delete Prefab And Scripts", FileMenuFlag.File)
            };

            // column 추가
            treeView.Columns.Insert(1, CreateConvertPrefabsColumn());

            // base type
            treeView.Columns.Insert(2, CreateSelectBaseTypeColumn());

            SetOriginPath(PrefabConvertSettings.OriginPath);
            treeView.ExpansionState.ExpandAll("PrefabWindow");
        }

        Column CreateConvertPrefabsColumn()
        {
            var column = new Column()
            {
                title = "Convert Prefab",
                width = 300f,
                minWidth = 200f,

                makeCell = () =>
                {
                    var template = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(new GUID(PrefabCellUxmlGUID));
                    var cell = template.Instantiate();

                    cell.AddManipulator(new ContextualMenuManipulator(OnPrefabCellContextMenu));

                    var objectField = cell.Q<ObjectField>("prefab");
                    cell.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        if (cell.userData is PrefabConvertData data)
                            treeView.SetSelectionById(data.id);
                    }, TrickleDown.TrickleDown);

                    return cell;
                },

                bindCell = (element, index) =>
                {
                    element.style.height = Length.Percent(100);

                    var data = treeView.GetItemDataForIndex<PrefabConvertData>(index);
                    Visibility visibility = data.isDirectory ? Visibility.Hidden : Visibility.Visible;

                    var statusField = element.Q<VisualElement>("status");
                    statusField.style.visibility = visibility;

                    var objectField = element.Q<ObjectField>("prefab");
                    objectField.style.visibility = visibility;
                    objectField.SetEnabled(false);

                    if (data.isDirectory == false)
                    {
                        RefreshStatus(statusField, data);
                        objectField.value = data.prefab;

                        element.userData = data;
                    }
                }
            };
            return column;
        }

        Column CreateSelectBaseTypeColumn()
        {
            var column = new Column()
            {
                title = "BaseType",
                width = 150f,
                minWidth = 100f,

                makeCell = () =>
                {
                    var template = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(new GUID(BaseTypeCellUxmlGUID));
                    var cell = template.Instantiate();

                    var dropdownField = cell.Q<DropdownField>("base_type");
                    dropdownField.RegisterValueChangedCallback(evt =>
                    {
                        if (cell.userData is PrefabConvertData data)
                        {
                            if (evt.newValue == PrefabConvertData.SelectTypesID)
                            {
                                SelectTypesWindow.Open(typeof(MonoBehaviour), result =>
                                {
                                    data.SelectBaseType(result);
                                    dropdownField.SetValueWithoutNotify(data.SelectedBaseTypeName);
                                });
                            }
                            else
                            {
                                data.SelectBaseType(evt.newValue);
                            }
                        }
                    });
                    return cell;
                },

                bindCell = (element, index) =>
                {
                    var data = treeView.GetItemDataForIndex<PrefabConvertData>(index);
                    element.style.height = Length.Percent(100);

                    var dropdownField = element.Q<DropdownField>("base_type");
                    dropdownField.style.visibility = data.isDirectory == false ? Visibility.Visible : Visibility.Hidden;

                    if (data.isDirectory == false)
                    {
                        dropdownField.choices = data.BaseTypeNames;
                        dropdownField.SetValueWithoutNotify(data.SelectedBaseTypeName);
                        dropdownField.SetEnabled(data.metadata == null || string.IsNullOrEmpty(data.metadata.baseType));

                        element.userData = data;
                    }
                }
            };
            return column;
        }

        void SetOriginPath(string originPath)
        {
            convertDatas.Clear();

            treeView.SetRootPath(originPath, new string[] { "*.prefab" }, (id, path) =>
            {
                var data = new PrefabConvertData(id, path);
                if (data.isDirectory == false)
                    convertDatas[data.originPath] = data;
                return data;
            }, true);
        }

        void RefreshConvertDatas()
        {
            foreach (var pair in convertDatas)
                pair.Value.Refresh();

            treeView.RefreshItems();
        }

        void RefreshStatus(VisualElement element, PrefabConvertData data)
        {
            switch (data.Status)
            {
                case PrefabConvertStatus.NotExistPrefab:
                    {
                        element.tooltip = data.Status.ToString();
                        element.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Idle);
                        break;
                    }
                case PrefabConvertStatus.InvalidMetadata:
                    {
                        element.tooltip = data.Status.ToString();
                        element.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Idle);
                        break;
                    }
                case PrefabConvertStatus.BaseTypeChanged:
                    {
                        element.tooltip = data.Status.ToString();
                        element.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Failed);
                        break;
                    }
                case PrefabConvertStatus.OriginModified:
                    {
                        element.tooltip = data.Status.ToString();
                        element.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Processing);
                        break;
                    }
                default:
                    {
                        element.tooltip = data.Status.ToString();
                        element.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Success);
                        break;
                    }
            }
        }

        void OnPrefabCellContextMenu(ContextualMenuPopulateEvent evt)
        {
            FileMenuFlag flag = 0;
            foreach (var id in treeView.SelectedIds)
            {
                var entry = treeView.GetItemDataForId<FileEntry>(id);
                flag |= entry.isDirectory ? FileMenuFlag.Folder : FileMenuFlag.File;
            }

            foreach (var item in prefabCellMenuItems)
            {
                if ((item.flag & flag) != 0)
                {
                    var actionName = item.name;
                    evt.menu.AppendAction(actionName, _ => OnContextMenuClicked(actionName));
                }
            }
        }

        void OnContextMenuClicked(string actionName)
        {
            switch (actionName)
            {
                case "Refresh":
                    {
                        RefreshConvertDatas();
                        break;
                    }
                case "Convert Prefab":
                    {
                        var id = treeView.SelectedIds.FirstOrDefault();
                        var data = treeView.GetItemDataForId<PrefabConvertData>(id);
                        ConvertPrefab(data);
                        break;
                    }
                case "Delete Prefab And Scripts":
                    {
                        var id = treeView.SelectedIds.FirstOrDefault();
                        var data = treeView.GetItemDataForId<PrefabConvertData>(id);

                        if (data.prefab != null)
                        {
                            if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete this?\n(Deleting the script may cause compilation errors.)", "Yes", "No") == false)
                                return;
                        }

                        DeletePrefabAndScripts(data);
                        break;
                    }
            }
        }

        void OnFileMouseDownEvent(MouseDownEvent evt, int id)
        {
            var data = treeView.GetItemDataForId<PrefabConvertData>(id);

            if (evt.button != 0)
                return;

            if (data.isDirectory)
            {
                var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(data.path);

                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                if (data.origin == null)
                    return;

                Selection.activeObject = data.origin;
                EditorGUIUtility.PingObject(data.origin);

                // 더블클릭 시 프리팹 열기
                if (evt.clickCount == 2)
                    AssetDatabase.OpenAsset(data.origin);
            }
            // 
            //evt.StopPropagation();
        }


        void ConvertPrefab(PrefabConvertData data)
        {
            treeView.ExpansionState.Save("PrefabWindow");
            PrefabConverter.StartProcess(data.originPath, data.SelectedBaseType);
        }

        public void OnConvertFinished(string originPath)
        {
            if (convertDatas.TryGetValue(originPath, out var data))
            {
                data.Refresh();
                treeView.RefreshItem(data.id);
                treeView.ExpansionState.ExpandAll("PrefabWindow");
            }
        }

        void DeletePrefabAndScripts(PrefabConvertData data)
        {
            string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, Path.GetDirectoryName(data.originPath));
            string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath, data.name);

            AssetDatabase.DeleteAsset(prefabPath);
            AssetDatabase.DeleteAsset(Path.Combine(PrefabConvertSettings.ScriptPath, relativePath, $"{data.origin.name}.cs"));
            AssetDatabase.DeleteAsset(Path.Combine(PrefabConvertSettings.ScriptPath, relativePath, $"{data.origin.name}.Impl.cs"));
            AssetDatabase.Refresh();

            data.Refresh();
            treeView.RefreshItem(data.id);
        }
    }
}