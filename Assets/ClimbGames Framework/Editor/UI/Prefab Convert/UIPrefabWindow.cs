using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.Android.Gradle;
using UnityEditor.UIElements;

namespace ClimbGames.Editor.UI
{
    public class UIPrefabWindow : EditorWindow
    {
        private const string WindowUxmlGUID = "8871c4d2fda088847a0c7fd3ac3c627e";
        private const string StateCellUxmlGUID = "74fa90ff22b5ca34abb41cc8a3468495";
        private const string OutputCellUxmlGUID = "5da4454cd02d87443a922c64360d0ae8";

        [MenuItem("Tools/ClimbGames/UI Prefab Convert")]
        private static void Open()
        {
            var window = GetWindow<UIPrefabWindow>();
            window.titleContent = new GUIContent("UI Prefab Convert");
            window.minSize = new Vector2(400, 300);
        }

        void CreateGUI()
        {
            string windowUxmlPath = AssetDatabase.GUIDToAssetPath(WindowUxmlGUID);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowUxmlPath);
            visualTree.CloneTree(rootVisualElement);

            // 프리팹 뷰
            var treeView = rootVisualElement.Q<FileTreeView>("prefab_view");

            var rawPathField = rootVisualElement.Q<SelectPathField>("raw_path");
            rawPathField.Path = UIPrefabConvertSettings.RawPath;
            rawPathField.OnPathChanged += (path) =>
            {
                UIPrefabConvertSettings.RawPath = path;
                treeView.SetRootPath(UIPrefabConvertSettings.RawPath, "*.prefab");
            };

            var outputPathField = rootVisualElement.Q<SelectPathField>("output_path");
            outputPathField.Path = UIPrefabConvertSettings.OutputPath;
            outputPathField.OnPathChanged += (path) =>
            {
                UIPrefabConvertSettings.OutputPath = path;
            };

            var codeGenPathField = rootVisualElement.Q<SelectPathField>("codegen_path");
            codeGenPathField.Path = UIPrefabConvertSettings.CodeGenPath;
            codeGenPathField.OnPathChanged += (path) =>
            {
                UIPrefabConvertSettings.CodeGenPath = path;
            };


            // 프리팹 뷰 column 추가
            string stateUxmlPath = AssetDatabase.GUIDToAssetPath(StateCellUxmlGUID);
            var stateCell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(stateUxmlPath);
            treeView.Columns.Insert(1, new Column()
            {
                title = "State",
                width = 150f,
                minWidth = 100f,

                makeCell = () => stateCell.Instantiate(),
                bindCell = (element, index) =>
                {
                    var entry = treeView.GetItemDataForIndex(index);
                    element.style.height = Length.Percent(100);

                    var stateButton = element.Q<Button>("state");
                    stateButton.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;

                    stateButton.clicked += () => OnClickConvert(entry.path);
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
                    objectField.style.visibility = entry.isDirectory ? Visibility.Hidden : Visibility.Visible;
                    objectField.SetEnabled(false);

                    //
                }
            });


            treeView.SetRootPath(UIPrefabConvertSettings.RawPath, "*.prefab");
            //
            //
        }

        void OnClickConvert(string path)
        {
            string relativePath = path.ToUnityRelativePath();
            UIPrefabConverter.StartProcess(relativePath);
        }
    }
}