using UnityEngine;
using UnityEditor;
using ExcelDataReader;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Compilation;
using System;
using System.Drawing;
using NUnit.Framework;
using GluonGui.Dialog;

namespace ClimbGames.Editor.Table
{
    [InitializeOnLoad]
    public static class TableConverter
    {
        private const string ReloadFlagKey = "CodeGen_IsWaitingForReload";
        private const string ConvertExcelFileKey = "Convert_ExcelFiles";

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
                EditorApplication.delayCall += () =>
                {
                    string value = SessionState.GetString(ConvertExcelFileKey, string.Empty);
                    if (string.IsNullOrEmpty(value) == false)
                        CreateTables(value.Split(';', StringSplitOptions.RemoveEmptyEntries));
                };
            }
        }

        public static void StartProcess(string[] excelFiles)
        {
            compilationFailed = false;

            if (GenerateTableCode(excelFiles))
            {
                SessionState.SetString(ConvertExcelFileKey, string.Join(";", excelFiles));

                CompilationPipeline.compilationFinished -= OnCompilationFinished;
                CompilationPipeline.compilationFinished += OnCompilationFinished;

                CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
                CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            }
            else
            {
                CreateTables(excelFiles);
            }
        }

        public static bool DeleteUnusedFiles()
        {
            var schemas = new List<Schema>();

            var allFiles = Paths.GetFiles(TableEditorSettings.ExcelPath, "*.xlsx", "*.xls");
            foreach (var path in allFiles)
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            var schema = new TableSchema(reader.Name);
                            schemas.Add(schema);
                        }
                        while (reader.NextResult());
                    }
                }
            }

            bool isChanged = false;
            string[] tableFiles = Paths.GetFiles(TableEditorSettings.DataPath, "*.cs", "*.asset", "*.bytes");
            foreach (var path in tableFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName == "TableEnum" || fileName == "Tables") continue;

                fileName = fileName.TrimEnd("Table", "TableRecord");
                if (schemas.Any(x => x.TableName == fileName) == false)
                    isChanged |= AssetDatabase.DeleteAsset(path.ToUnityRelativePath());
            }

            return isChanged;
        }

        static bool GenerateTableCode(string[] excelFiles)
        {
            var schemas = new List<Schema>();
            var enumSchema = new EnumSchema();
            var loadSchema = new LoadSchema();

            enumSchema.ReadDeclaredEnum(TableEditorSettings.CodeGenPath);
            schemas.Add(enumSchema);
            schemas.Add(loadSchema);

            var nameHash = excelFiles.Select(x => Path.GetFileName(x)).ToHashSet();
            var allFiles = Paths.GetFiles(TableEditorSettings.ExcelPath, "*.xlsx", "*.xls");
            foreach (var filePath in allFiles)
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            bool result = false;
                            var schema = new TableSchema(reader.Name, enumSchema);
                            if (nameHash.Contains(Path.GetFileName(filePath)))
                            {
                                if (result = schema.Resolve(reader))
                                    schemas.Add(schema);
                            }
                            else
                            {
                                result = schema.Read(reader);
                            }
                            if (result)
                            {
                                loadSchema.AddTableSchema(schema);
                            }
                            else
                            {
                                Debug.Log($"[Tables] {schema.TableName} table header is invalid...");
                            }
                        }
                        while (reader.NextResult());
                    }
                }
            }

            if (TableCodeGenerator.Write(TableEditorSettings.CodeGenPath, schemas))
            {
                AssetDatabase.Refresh();
                return true;
            }

            return false;
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

        static List<ClimbGames.Table> CreateTables(string[] excelFiles)
        {
            List<ClimbGames.Table> tables = new List<ClimbGames.Table>();
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
                                try
                                {
                                    string dataPath = TableEditorSettings.DataPath;
                                    ClimbGames.Table table = TableData.Get(schema).CreateAsset(reader, dataPath);
                                    tables.Add(table);
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log($"[Tables] fail create table({schema.TableName}): {ex}");
                                }
                            }
                        }
                        while (reader.NextResult());
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[Tables] convert success.");

            if (EditorWindow.HasOpenInstances<TableWindow>())
            {
                var window = EditorWindow.GetWindow<TableWindow>();
                window?.OnConvertFinished(tables);
            }

            return tables;
        }

        public static void SaveToBytes(List<ClimbGames.Table> tables, string path)
        {
            string directoryPath = Path.Combine(path, "Bytes");
            Directory.CreateDirectory(directoryPath);

            foreach (var table in tables)
            {
                string filePath = Path.Combine(directoryPath, $"{table.name}.bytes");
                File.WriteAllBytes(filePath, table.ToBytes());
            }

            AssetDatabase.Refresh();
        }
    }
}