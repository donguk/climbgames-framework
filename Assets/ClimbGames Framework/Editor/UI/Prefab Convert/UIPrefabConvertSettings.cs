using ClimbGames.Editor;
using UnityEditor;

namespace ClimbGames.Editor
{
    public static class UIPrefabConvertSettings
    {
        private static string rawPath;
        private static string outputPath;
        private static string codeGenPath;

        public static string RawPath
        {
            get => rawPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIPrefabConvertSettings)}_{nameof(rawPath)}", rawPath = value);
        }
        public static string OutputPath
        {
            get => outputPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIPrefabConvertSettings)}_{nameof(outputPath)}", outputPath = value);
        }
        public static string CodeGenPath
        {
            get => codeGenPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIPrefabConvertSettings)}_{nameof(codeGenPath)}", codeGenPath = value);
        }

        static UIPrefabConvertSettings()
        {
            Reset();

            rawPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIPrefabConvertSettings)}_{nameof(rawPath)}", "Assets/Prefabs/UI");
            outputPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIPrefabConvertSettings)}_{nameof(outputPath)}", "Assets/Sources/Prefabs/UI");
            codeGenPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(UIPrefabConvertSettings)}_{nameof(codeGenPath)}", "Assets/Scripts/UI/CodeGen");
        }

        public static void Reset()
        {
            RawPath = "Assets/Prefabs/UI";
            outputPath = "Assets/Sources/Prefabs/UI";
            codeGenPath = "Assets/Scripts/UI/CodeGen";
        }
    }
}