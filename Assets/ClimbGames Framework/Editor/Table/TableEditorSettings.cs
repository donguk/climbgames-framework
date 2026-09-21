using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public static class TableEditorSettings
    {
        private static string EditorKey => $"{Application.dataPath.GetHashCode()}";
        private static string dataPath;
        private static string codePath;

        public static string DataPath
        {
            get => dataPath;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(dataPath)}", value);
        }

        public static string CodeGenPath
        {
            get => codePath;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(codePath)}", value);
        }

        static TableEditorSettings()
        {
            dataPath = PlayerPrefs.GetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(dataPath)}", "Assets/Tables");
            codePath = PlayerPrefs.GetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(codePath)}", "Assets/Tables/CodeGen");
        }
    }
}