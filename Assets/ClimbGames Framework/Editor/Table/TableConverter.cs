using UnityEngine;
using UnityEditor;
using ExcelDataReader;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.CodeDom.Compiler;

namespace ClimbGames.Editor.Table
{
    [InitializeOnLoad]
    public static class TableConverter
    {
        private static string dataPath = "Assets/Tables";
        private static string scriptPath = "Assets/Tables/Scripts";
        private const string ReloadFlagKey = "CodeGen_IsWaitingForReload";

        public static string DataPath = dataPath;
        public static string CodeGeneratePath => scriptPath;

        static TableConverter()
        {
            if (SessionState.GetBool(ReloadFlagKey, false))
            {
                SessionState.SetBool(ReloadFlagKey, false);
                // 리로드 직후 내부 상태가 완전히 안정될 때까지 한 프레임 지연 후 실행
                EditorApplication.delayCall += OnAfterGenerateCode;
            }
        }

        [MenuItem("Tools/ClimbGames/Table Convert")]
        static void StartConvert()
        {
            GenerateCodes();

            AssetDatabase.Refresh();
            SessionState.SetBool(ReloadFlagKey, true);
        }

        static void OnAfterGenerateCode()
        {
            CreateAssets();
            AssetDatabase.Refresh();
        }

        static string[] FindExcelFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*.xlsx");
            return files.Concat(Directory.GetFiles(path, "*.xls")).ToArray();
        }

        static void GenerateCodes()
        {
            string[] excelFiles = FindExcelFiles(Path.Combine(Directory.GetCurrentDirectory(), "Tables"));
            foreach (var filePath in excelFiles)
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            var schema = new TableSchema(reader.Name);
                            if (schema.Resolve(reader))
                                CodeGenerator.Get(schema).Write(scriptPath);
                            else
                                Debug.LogError($"[Tables] {schema.TableName} table schema is invalid");
                        }
                        while (reader.NextResult());
                    }
                }
            }
        }

        static void CreateAssets()
        {
            string[] excelFiles = FindExcelFiles(Path.Combine(Directory.GetCurrentDirectory(), "Tables"));
            foreach (var filePath in excelFiles)
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            var schema = new TableSchema(reader.Name);
                            if (schema.Read(reader))
                            {
                                TableData.Get(schema).CreateAsset(reader, dataPath);
                            }
                        }
                        while (reader.NextResult());
                    }
                }
            }
        }
    }
}