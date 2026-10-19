using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;
using System;
using ClimbGames.UI;
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

        [MenuItem("Tools/ClimbGames/UI Prefab Convert")]
        private static void Open()
        {
            var window = GetWindow<PrefabWindow>();
            window.titleContent = new GUIContent("UI Prefab Convert");
            window.minSize = new Vector2(400, 300);
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
                new FileMenuItem("Convert", FileMenuFlag.File),
                new FileMenuItem("Clear", FileMenuFlag.File)
            });
            treeView.onContextMenuClicked += OnContextMenuClicked;

            // column 추가
            treeView.Columns.Insert(1, CreateConvertPrefabsColumn());

            // base type
            treeView.Columns.Insert(2, CreateSelectBaseTypeColumn());

            //string convertUxmlPath = AssetDatabase.GUIDToAssetPath(ConvertCellUxmlGUID);
            //var convertCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(convertUxmlPath);
            //treeView.Columns.Insert(4, new Column()
            //{
            //    title = "Convert",
            //    width = 80f,
            //    minWidth = 80f,
            //
            //    makeCell = () =>
            //    {
            //        var cell = convertCell.Instantiate();
            //        var convertButton = cell.Q<Button>("convert");
            //
            //        convertButton.clicked += () =>
            //        {
            //            if (cell.userData is FileEntry entry)
            //                OnClickConvert(entry.path);
            //        };
            //
            //
            //        cell.AddManipulator(new ContextualMenuManipulator(evt =>
            //        {
            //            if (cell.userData is not FileEntry data)
            //                return;
            //
            //            evt.menu.AppendAction("Rename", _ =>
            //            {
            //                Debug.Log($"Rename: {data.name}");
            //            });
            //
            //            evt.menu.AppendAction("Delete", _ =>
            //            {
            //                Debug.Log($"Delete: {data.name}");
            //            });
            //        }));
            //
            //        return cell;
            //    },
            //    bindCell = (element, index) =>
            //    {
            //        var entry = treeView.GetItemDataForIndex(index);
            //        element.style.height = Length.Percent(100);
            //
            //        var convertButton = element.Q<Button>("convert");
            //        convertButton.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;
            //
            //        element.userData = entry;
            //    }
            //});

            SetOriginPath(PrefabConvertSettings.OriginPath);
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

                    cell.AddManipulator(new ContextualMenuManipulator(treeView.OnContextMenu));
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
                    var baseTypeCell = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(new GUID(BaseTypeCellUxmlGUID));
                    return baseTypeCell.Instantiate();
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

                        dropdownField.formatSelectedValueCallback = (selected) =>
                        {
                            if (selected == PrefabConvertData.SelectTypesID)
                            {
                                SelectTypesWindow.Open(typeof(MonoBehaviour), result =>
                                {
                                    data.SelectBaseType(result);
                                    dropdownField.SetValueWithoutNotify(data.SelectedBaseTypeName);
                                    treeView.RefreshItem(data.id);
                                });
                            }
                            else
                            {
                                dropdownField.index = data.SelectBaseType(selected);
                                treeView.RefreshItem(data.id);
                            }
                            return selected;
                        };
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
                pair.Value.RefreshPrefab();

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

        void OnContextMenuClicked(string actionName)
        {
            switch (actionName)
            {
                case "Refresh":
                    {
                        RefreshConvertDatas();
                        break;
                    }

                case "Convert":
                    {
                        var id = treeView.SelectedIds.FirstOrDefault();
                        var data = treeView.GetItemDataForId<PrefabConvertData>(id);
                        ConvertPrefab(data);
                        break;
                    }

                case "Clear":
                    {
                        var id = treeView.SelectedIds.FirstOrDefault();
                        var data = treeView.GetItemDataForId<PrefabConvertData>(id);
                        ClearPrefabFiles(data);
                        break;
                    }
            }
        }

        void ConvertPrefab(PrefabConvertData data)
        {
            PrefabConverter.StartProcess(data.originPath, data.SelectedBaseType);
        }

        public void OnConvertFinished(string originPath)
        {
            if (convertDatas.TryGetValue(originPath, out var data))
            {
                data.RefreshPrefab();
                treeView.RefreshItem(data.id);
            }
        }

        void ClearPrefabFiles(PrefabConvertData data)
        {
            string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, Path.GetDirectoryName(data.originPath));
            string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath, data.name);

            AssetDatabase.DeleteAsset(prefabPath);
            AssetDatabase.DeleteAsset(Path.Combine(PrefabConvertSettings.ScriptPath, relativePath, $"{data.origin.name}.cs"));
            AssetDatabase.DeleteAsset(Path.Combine(PrefabConvertSettings.ScriptPath, relativePath, $"{data.origin.name}.Impl.cs"));
            AssetDatabase.Refresh();

            data.RefreshPrefab();
            treeView.RefreshItem(data.id);
        }
    }
}