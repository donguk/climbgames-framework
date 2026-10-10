using UnityEditor;

namespace ClimbGames.Editor.UI
{
    public static class PrefabConvertSettings
    {
        private static string rawPath;
        private static string outputPath;
        private static string codeGenPath;

        public static string RawPath
        {
            get => rawPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(PrefabConvertSettings)}_{nameof(rawPath)}", rawPath = value);
        }
        public static string OutputPath
        {
            get => outputPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(PrefabConvertSettings)}_{nameof(outputPath)}", outputPath = value);
        }
        public static string CodeGenPath
        {
            get => codeGenPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(PrefabConvertSettings)}_{nameof(codeGenPath)}", codeGenPath = value);
        }

        static PrefabConvertSettings()
        {
            rawPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(PrefabConvertSettings)}_{nameof(rawPath)}", "Assets/Prefabs/UI");
            outputPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(PrefabConvertSettings)}_{nameof(outputPath)}", "Assets/Sources/Prefabs/UI");
            codeGenPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(PrefabConvertSettings)}_{nameof(codeGenPath)}", "Assets/Scripts/UI/CodeGen");
        }

        public static void Reset()
        {
            RawPath = "Assets/Prefabs/UI";
            outputPath = "Assets/Sources/Prefabs/UI";
            codeGenPath = "Assets/Scripts/UI/CodeGen";
        }
    }
}