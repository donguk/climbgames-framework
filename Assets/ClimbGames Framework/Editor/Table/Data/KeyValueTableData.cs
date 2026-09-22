using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExcelDataReader;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public class KeyValueTableData : TableData
    {
        private TableSchema schema;

        public KeyValueTableData(TableSchema schema)
        {
            this.schema = schema;
        }

        public override void CreateAsset(IExcelDataReader reader, string path)
        {
            var columns = schema.Header.Columns;
            if (columns.Count > 1)
            {
                var keyColumn = columns[0];

                var tableType = Type.GetType($"{schema.Namespace}.{schema.TableName}Table, Assembly-CSharp");
                var listInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var methodInfo = listInfo.FieldType.GetMethod("Add");

                var dictionaryInfo = tableType.GetField("dictionary", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var keyType = dictionaryInfo.FieldType.GetGenericArguments()[0];

                var list = Activator.CreateInstance(listInfo.FieldType);
                HashSet<object> keyHash = new HashSet<object>();

                while (reader.Read())
                {
                    try
                    {
                        var key = reader.GetValue(keyColumn.Index);
                        var convertedKey = ConvertValue(key, keyType);
                        if (keyHash.Add(convertedKey) == false)
                        {
                            Debug.LogError($"[TableData] {schema.TableName}: duplicated key({convertedKey})/ depth({reader.Depth})");
                            continue;
                        }

                        var record = ReadRecord(reader, schema);
                        methodInfo.Invoke(list, new[] { record });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TableData] {schema.TableName}: fail add depth({reader.Depth})/ ex({ex.Message})");
                        continue;
                    }
                }

                var tableAsset = ScriptableObject.CreateInstance(tableType);
                listInfo.SetValue(tableAsset, list);

                AssetDatabase.CreateAsset(tableAsset, Path.Combine(path, $"{schema.TableName}.asset"));
            }
        }
    }
}