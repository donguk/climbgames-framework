using UnityEngine;
using UnityEditor;
using ExcelDataReader;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Compilation;
using System;

namespace ClimbGames.Editor.Table
{
    [InitializeOnLoad]
    public static class TableConverter
    {
        private const string ReloadFlagKey = "CodeGen_IsWaitingForReload";
        private static bool compilationFailed;

        static TableConverter()
        {
            compilationFailed = false;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

            if (SessionState.GetBool(ReloadFlagKey, false))
            {
                SessionState.SetBool(ReloadFlagKey, false);

                // 리로드 직후 내부 상태가 완전히 안정될 때까지 한 프레임 지연 후 실행
                EditorApplication.delayCall += OnGenerateCodeCompleted;
            }
        }

        public static void StartConvert()
        {
            compilationFailed = false;

            if (GenerateTableCodes())
            {
                AssetDatabase.Refresh();

                CompilationPipeline.compilationFinished -= OnCompilationFinished;
                CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

                CompilationPipeline.compilationFinished += OnCompilationFinished;
                CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            }
            else
            {
                OnGenerateCodeCompleted();
            }
        }

        static void OnGenerateCodeCompleted()
        {
            try
            {
                CreateTableAssets();

                AssetDatabase.Refresh();
                Debug.Log("[Tables] convert success.");

                if (EditorWindow.HasOpenInstances<TableWindow>())
                {
                    var window = EditorWindow.GetWindow<TableWindow>();
                    window?.OnConvertFinished();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Tables] convert fail: {ex}");
            }
        }

        static string[] FindExcelFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*.xlsx");
            return files.Concat(Directory.GetFiles(path, "*.xls")).ToArray();
        }

        static bool GenerateTableCodes()
        {
            var schemas = new List<ISchema>();
            var enumSchema = new EnumSchema();
            schemas.Add(enumSchema);

            string[] excelFiles = FindExcelFiles(TableEditorSettings.ExcelPath);
            foreach (var filePath in excelFiles)
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            var schema = new TableSchema(reader.Name, enumSchema);
                            if (schema.Resolve(reader))
                            {
                                schemas.Add(schema);
                            }
                            else
                            {
                                Debug.LogError($"[Tables] {schema.TableName} table schema is invalid");
                            }
                        }
                        while (reader.NextResult());
                    }
                }
            }

            return TableCodeGenerator.Write(TableEditorSettings.CodeGenPath, schemas);
        }

        static void OnCompilationFinished(object context)
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

            if (compilationFailed)
                return;

            SessionState.SetBool(ReloadFlagKey, true);
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    Debug.LogError(
                            $"[TableCodeGen] Compile Error\n" +
                            $"Assembly: {assemblyPath}\n" +
                            $"File: {message.file}\n" +
                            $"Line: {message.line}\n" +
                            $"Column: {message.column}\n" +
                            $"{message.message}");

                    compilationFailed = true;
                    break;
                }
            }
        }

        static void CreateTableAssets()
        {
            string[] excelFiles = FindExcelFiles(TableEditorSettings.ExcelPath);
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
                                ClimbGames.Table table = TableData.Get(schema).CreateAsset(reader, TableEditorSettings.DataPath);
                                if (TableEditorSettings.SaveToBytes)
                                {
                                    string directoryPath = Path.Combine(TableEditorSettings.DataPath, "Bytes");
                                    Directory.CreateDirectory(directoryPath);

                                    string savePath = Path.Combine(directoryPath, $"{schema.TableName}.bytes");
                                    File.WriteAllBytes(savePath, table.ToBytes());
                                }
                            }
                        }
                        while (reader.NextResult());
                    }
                }
            }
        }
    }
}