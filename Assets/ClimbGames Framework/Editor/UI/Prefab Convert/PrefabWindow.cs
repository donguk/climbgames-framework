using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;

namespace ClimbGames.Editor.UI
{
    public class PrefabWindow : EditorWindow
    {
        public class ConvertData
        {
            public FileEntry entry;
            public GameObject prefab;
        }

        private const string WindowUxmlGUID = "8871c4d2fda088847a0c7fd3ac3c627e";
        private const string StateCellUxmlGUID = "74fa90ff22b5ca34abb41cc8a3468495";
        private const string OutputCellUxmlGUID = "5da4454cd02d87443a922c64360d0ae8";

        private FileTreeView treeView;
        private Dictionary<string, ConvertData> convertDatas = new Dictionary<string, ConvertData>();

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
                treeView.SetRootPath(relativePath, "*.prefab");
                return PrefabConvertSettings.OriginPath = relativePath;
            };

            var outputPathField = rootVisualElement.Q<SelectPathField>("output_path");
            outputPathField.Path = PrefabConvertSettings.OutputPath;
            outputPathField.OnPathChanged += (path) =>
            {
                string relativePath = path.ToUnityRelativePath();
                RefreshConvertDatas();
                treeView.RefreshItems();
                return PrefabConvertSettings.OutputPath = relativePath;
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
                title = "State",
                width = 150f,
                minWidth = 100f,

                makeCell = () =>
                {
                    var cell = stateCell.Instantiate();
                    var stateButton = cell.Q<Button>("state");

                    stateButton.clicked += () =>
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

                    var stateButton = element.Q<Button>("state");
                    stateButton.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;

                    element.userData = entry;
                }
            });

            string outputUxmlPath = AssetDatabase.GUIDToAssetPath(OutputCellUxmlGUID);
            var outputCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(outputUxmlPath);
            treeView.Columns.Insert(2, new Column()
            {
                title = "Output",
                width = 300f,
                minWidth = 200f,

                makeCell = () => outputCell.Instantiate(),
                bindCell = (element, index) =>
                {
                    var entry = treeView.GetItemDataForIndex(index);
                    element.style.height = Length.Percent(100);

                    var objectField = element.Q<ObjectField>("output");
                    if (entry.isDirectory == false)
                    {
                        if (convertDatas.TryGetValue(entry.path.ToUnityRelativePath(), out var data))
                            objectField.value = data.prefab;
                        else
                            objectField.value = null;
                        objectField.style.visibility = Visibility.Visible;
                    }
                    else
                    {
                        objectField.style.visibility = Visibility.Hidden;
                    }
                    objectField.SetEnabled(false);
                }
            });

            treeView.SetRootPath(PrefabConvertSettings.OriginPath, "*.prefab");
            RefreshConvertDatas();
        }

        void RefreshConvertDatas()
        {
            convertDatas.Clear();
            using (new ListPoolScope<FileEntry>(out var list))
            {
                treeView.GetFileItemDatas(ref list);

                foreach (var entry in list)
                {
                    string originPath = entry.path.ToUnityRelativePath();
                    string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, originPath);
                    string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath).Replace("\\", "/");

                    convertDatas[originPath] = new ConvertData()
                    {
                        entry = entry,
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath),
                    };
                }
            }
        }

        void OnClickConvert(string path)
        {
            string relativePath = path.ToUnityRelativePath();
            PrefabConverter.StartProcess(relativePath);
        }

        public void OnConvertFinished(string originPath)
        {
            if (convertDatas.TryGetValue(originPath, out var data))
            {
                string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, originPath);
                string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath).Replace("\\", "/");
                data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                treeView.RefreshItem(data.entry.id);
            }
        }
    }
}