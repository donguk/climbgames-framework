using ClimbGames.Editor;
using UnityEditor;

namespace ClimbGames.Editor
{
    public static class UIEditorSettings
    {
        private static string prefabPath;
        private static string sourcePath;
        private static string codeGenPath;

        public static string PrefabPath
        {
            get => prefabPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIEditorSettings)}_{nameof(prefabPath)}", prefabPath = value);
        }
        public static string SourcePath
        {
            get => sourcePath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIEditorSettings)}_{nameof(sourcePath)}", sourcePath = value);
        }
        public static string CodeGenPath
        {
            get => codeGenPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIEditorSettings)}_{nameof(codeGenPath)}", codeGenPath = value);
        }

        static UIEditorSettings()
        {
            prefabPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIEditorSettings)}_{nameof(prefabPath)}", "Assets/Prefabs/UI");
            sourcePath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIEditorSettings)}_{nameof(sourcePath)}", "Assets/Sources/Prefabs/UI");
            codeGenPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIEditorSettings)}_{nameof(codeGenPath)}", "Assets/Scripts/UI/CodeGen");
        }

        public static void Reset()
        {
            PrefabPath = "Assets/UI/Prefabs";
            sourcePath = "Assets/Sources/Prefabs/UI";
            codeGenPath = "Assets/Scripts/UI/CodeGen";
        }
    }
}