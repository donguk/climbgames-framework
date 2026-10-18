using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;
using System;
using ClimbGames.UI;

namespace ClimbGames.Editor.UI
{
    public class PrefabWindow : EditorWindow
    {
        private const string WindowUxmlGUID = "8871c4d2fda088847a0c7fd3ac3c627e";
        private const string StateCellUxmlGUID = "74fa90ff22b5ca34abb41cc8a3468495";
        private const string OutputCellUxmlGUID = "5da4454cd02d87443a922c64360d0ae8";
        private const string BaseTypeCellUxmlGUID = "67648f2031ce73a46997d10736f7cc65";
        private const string ConvertCellUxmlGUID = "37de53add6cead84293eb14086d0eb18";

        private FileTreeView treeView;
        private Dictionary<string, PrefabConvertInfo> convertInfos = new Dictionary<string, PrefabConvertInfo>();

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

            // 프리팹 뷰
            treeView = rootVisualElement.Q<FileTreeView>("prefab_view");

            var rawPathField = rootVisualElement.Q<SelectPathField>("raw_path");
            rawPathField.Path = PrefabConvertSettings.OriginPath;
            rawPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                PrefabConvertSettings.OriginPath = relativePath;

                var root = FileTreeView.BuildRoot(relativePath, "*.prefab");
                using (new ListPoolScope<FileEntry>(out var list))
                {
                    FileTreeView.GetFileItemDatas(root, ref list);
                    RefreshConvertDatas(list);
                }
                treeView.SetRootItem(root);
                treeView.RefreshItems();
                return relativePath;
            };

            var outputPathField = rootVisualElement.Q<SelectPathField>("output_path");
            outputPathField.Path = PrefabConvertSettings.OutputPath;
            outputPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                PrefabConvertSettings.OutputPath = relativePath;

                RefreshConvertDatas();
                treeView.RefreshItems();
                return relativePath;
            };

            var codeGenPathField = rootVisualElement.Q<SelectPathField>("codegen_path");
            codeGenPathField.Path = PrefabConvertSettings.ScriptPath;
            codeGenPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                return PrefabConvertSettings.ScriptPath = relativePath;
            };


            // 프리팹 뷰 column 추가
            string stateUxmlPath = AssetDatabase.GUIDToAssetPath(StateCellUxmlGUID);
            var stateCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(stateUxmlPath);
            treeView.Columns.Insert(1, new Column()
            {
                title = "Status",
                width = 50f,
                minWidth = 50f,
                resizable = false,

                makeCell = () => stateCell.Instantiate(),
                bindCell = (element, index) =>
                {
                    var entry = treeView.GetItemDataForIndex(index);
                    element.style.height = Length.Percent(100);

                    var status = element.Q<VisualElement>("status");
                    status.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;

                    if (entry.isDirectory == false)
                    {
                        convertInfos.TryGetValue(entry.path.ToUnityRelativePath(), out var info);
                        switch (info.Status)
                        {
                            case PrefabConvertStatus.NotExistPrefab:
                                {
                                    status.tooltip = info.Status.ToString();
                                    status.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Idle);
                                    break;
                                }
                            case PrefabConvertStatus.InvalidMetadata:
                                {
                                    status.tooltip = info.Status.ToString();
                                    status.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Idle);
                                    break;
                                }
                            case PrefabConvertStatus.BaseTypeChanged:
                                {
                                    status.tooltip = info.Status.ToString();
                                    status.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Failed);
                                    break;
                                }
                            case PrefabConvertStatus.OriginModified:
                                {
                                    status.tooltip = info.Status.ToString();
                                    status.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Processing);
                                    break;
                                }
                            default:
                                {
                                    status.tooltip = info.Status.ToString();
                                    status.style.backgroundImage = new StyleBackground(PrefabStatusIcons.Success);
                                    break;
                                }
                        }
                    }
                }
            });

            string outputUxmlPath = AssetDatabase.GUIDToAssetPath(OutputCellUxmlGUID);
            var outputCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(outputUxmlPath);
            treeView.Columns.Insert(2, new Column()
            {
                title = "Output",
                width = 200f,
                minWidth = 100f,

                makeCell = () => outputCell.Instantiate(),
                bindCell = (element, index) =>
                {
                    var entry = treeView.GetItemDataForIndex(index);
                    element.style.height = Length.Percent(100);

                    var objectField = element.Q<ObjectField>("output");
                    objectField.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;

                    if (entry.isDirectory == false)
                    {
                        convertInfos.TryGetValue(entry.path.ToUnityRelativePath(), out var info);
                        objectField.value = info.prefab;
                    }
                    objectField.SetEnabled(false);
                }
            });



            // base type
            string baseTypeUxmlPath = AssetDatabase.GUIDToAssetPath(BaseTypeCellUxmlGUID);
            var baseTypeCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(baseTypeUxmlPath);
            treeView.Columns.Insert(3, new Column()
            {
                title = "BaseType",
                width = 150f,
                minWidth = 100f,

                makeCell = () => baseTypeCell.Instantiate(),
                bindCell = (element, index) =>
                {
                    var entry = treeView.GetItemDataForIndex(index);
                    element.style.height = Length.Percent(100);

                    var dropdownField = element.Q<DropdownField>("base_type");
                    dropdownField.style.visibility = entry.isDirectory == false ? Visibility.Visible : Visibility.Hidden;

                    if (entry.isDirectory == false)
                    {
                        convertInfos.TryGetValue(entry.path.ToUnityRelativePath(), out var info);
                        dropdownField.choices = info.BaseTypeNames;
                        dropdownField.SetValueWithoutNotify(info.SelectedBaseTypeName);

                        dropdownField.formatSelectedValueCallback = (selected) =>
                        {
                            if (selected == PrefabConvertInfo.SelectTypesID)
                            {
                                SelectTypesWindow.Open(typeof(MonoBehaviour), result =>
                                {
                                    info.SelectBaseType(result);
                                    dropdownField.SetValueWithoutNotify(info.SelectedBaseTypeName);
                                    treeView.RefreshItem(info.entry.id);
                                });
                            }
                            else
                            {
                                dropdownField.index = info.SelectBaseType(selected);
                                treeView.RefreshItem(info.entry.id);
                            }
                            return selected;
                        };
                    }
                }
            });

            string convertUxmlPath = AssetDatabase.GUIDToAssetPath(ConvertCellUxmlGUID);
            var convertCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(convertUxmlPath);
            treeView.Columns.Insert(4, new Column()
            {
                title = "Convert",
                width = 80f,
                minWidth = 80f,

                makeCell = () =>
                {
                    var cell = convertCell.Instantiate();
                    var convertButton = cell.Q<Button>("convert");

                    convertButton.clicked += () =>
                    {
                        if (cell.userData is FileEntry entry)
                            OnClickConvert(entry.path);
                    };

                    return cell;
                },
                bindCell = (element, index) =>
                {
                    var entry = treeView.GetItemDataForIndex(index);
                    element.style.height = Length.Percent(100);

                    var convertButton = element.Q<Button>("convert");
                    convertButton.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;

                    element.userData = entry;
                }
            });

            treeView.SetRootPath(PrefabConvertSettings.OriginPath, "*.prefab");
            RefreshConvertDatas();
        }

        void RefreshConvertDatas()
        {
            using (new ListPoolScope<FileEntry>(out var list))
            {
                treeView.GetFileItemDatas(ref list);
                RefreshConvertDatas(list);
            }
        }

        void RefreshConvertDatas(List<FileEntry> list)
        {
            convertInfos.Clear();
            foreach (var entry in list)
            {
                string originPath = entry.path.ToUnityRelativePath();
                string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, originPath);
                string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath).Replace("\\", "/");

                convertInfos[originPath] = new PrefabConvertInfo(entry, AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            }
        }

        void OnClickConvert(string path)
        {
            string relativePath = path.ToUnityRelativePath();
            if (convertInfos.TryGetValue(relativePath, out var info))
                PrefabConverter.StartProcess(relativePath, info.SelectedBaseType);
        }

        public void OnConvertFinished(string originPath)
        {
            if (convertInfos.TryGetValue(originPath, out var info))
            {
                string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, originPath);
                string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath).Replace("\\", "/");
                info.Setup(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));

                treeView.RefreshItem(info.entry.id);
            }
        }
    }
}