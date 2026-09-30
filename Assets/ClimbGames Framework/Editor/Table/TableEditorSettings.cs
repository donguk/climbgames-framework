using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public static class TableEditorSettings
    {
        private static string EditorKey => $"{Application.dataPath.GetHashCode()}";
        private static string excelPath;
        private static string dataPath;
        private static string codeGenPath;

        public static bool ReadHeaderEnumValues => true;

        public static string ExcelPath
        {
            get => excelPath;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(excelPath)}", value);
        }
        public static string DataPath
        {
            get => dataPath;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(dataPath)}", value);
        }

        public static string CodeGenPath
        {
            get => codeGenPath;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(codeGenPath)}", value);
        }

        static TableEditorSettings()
        {
            string defatulExcelPath = Path.Combine(Directory.GetCurrentDirectory(), "Excels");
            excelPath = PlayerPrefs.GetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(excelPath)}", defatulExcelPath);
            dataPath = PlayerPrefs.GetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(dataPath)}", "Assets\\Tables");
            codeGenPath = PlayerPrefs.GetString($"{EditorKey}_{nameof(TableEditorSettings)}_{nameof(codeGenPath)}", "Assets\\Tables\\CodeGen");
        }
    }
}