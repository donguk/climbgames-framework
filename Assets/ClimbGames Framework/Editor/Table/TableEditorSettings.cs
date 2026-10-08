using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public static class TableEditorSettings
    {
        private static string excelPath;
        private static string dataPath;
        private static string codeGenPath;
        private static bool saveToBytes;

        public static bool ReadHeaderEnumValues => true;

        public static string ExcelPath
        {
            get => excelPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(excelPath)}", excelPath = value);
        }
        public static string DataPath
        {
            get => dataPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(dataPath)}", dataPath = value);
        }
        public static string CodeGenPath
        {
            get => codeGenPath;
            set => EditorPrefs.SetString($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(codeGenPath)}", codeGenPath = value);
        }
        public static bool SaveToBytes
        {
            get => saveToBytes;
            set => EditorPrefs.SetBool($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(saveToBytes)}", saveToBytes = value);
        }

        static TableEditorSettings()
        {
            string defatulExcelPath = Path.Combine(Directory.GetCurrentDirectory(), "Excels");
            excelPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(excelPath)}", defatulExcelPath);
            dataPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(dataPath)}", "Assets/Tables");
            codeGenPath = EditorPrefs.GetString($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(codeGenPath)}", "Assets/Tables/CodeGen");
            saveToBytes = EditorPrefs.GetBool($"{FrameworkEditorSettings.EditorKey}_{nameof(TableEditorSettings)}_{nameof(saveToBytes)}", false);
        }

        public static void Reset()
        {
            ExcelPath = Path.Combine(Directory.GetCurrentDirectory(), "Excels");
            DataPath = "Assets/Tables";
            CodeGenPath = "Assets/Tables/CodeGen";
            SaveToBytes = false;
        }
    }
}