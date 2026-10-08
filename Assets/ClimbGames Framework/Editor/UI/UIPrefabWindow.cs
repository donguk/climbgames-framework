using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace ClimbGames.Editor
{
    public class UIPrefabWindow : EditorWindow
    {
        private const string UxmlGUID = "8871c4d2fda088847a0c7fd3ac3c627e";

        [MenuItem("Tools/ClimbGames/UI Prefab Convert")]
        private static void Open()
        {
            var window = GetWindow<UIPrefabWindow>();
            window.titleContent = new GUIContent("UI Prefab Convert");
            window.minSize = new Vector2(400, 300);
        }

        void CreateGUI()
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath(UxmlGUID);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            visualTree.CloneTree(rootVisualElement);

            var prefabPath = rootVisualElement.Q<SelectPathField>("prefab_path");
            prefabPath.Path = UIEditorSettings.PrefabPath;
            prefabPath.OnPathChanged += (path) =>
            {
                UIEditorSettings.PrefabPath = path;
            };

            var sourcePath = rootVisualElement.Q<SelectPathField>("source_path");
            sourcePath.Path = UIEditorSettings.SourcePath;
            sourcePath.OnPathChanged += (path) =>
            {
                UIEditorSettings.SourcePath = path;
            };

            var codeGenPath = rootVisualElement.Q<SelectPathField>("codegen_path");
            codeGenPath.Path = UIEditorSettings.CodeGenPath;
            codeGenPath.OnPathChanged += (path) =>
            {
                UIEditorSettings.CodeGenPath = path;
            };
        }
    }
}