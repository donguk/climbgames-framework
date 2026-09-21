using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExcelDataReader;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public class DictionaryTableData : TableData
    {
        private TableSchema schema;

        public DictionaryTableData(TableSchema schema)
        {
            this.schema = schema;
        }

        public override void CreateAsset(IExcelDataReader reader, string path)
        {
            var keyColumn = schema.Header.KeyColumn;
            if (keyColumn != null)
            {
                var tableType = Type.GetType($"{schema.Namespace}.{schema.TableName}Table, Assembly-CSharp");
                var fieldInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var methodInfo = fieldInfo.FieldType.GetMethod("Add");

                var keyType = fieldInfo.FieldType.GetGenericArguments()[0];

                var datas = Activator.CreateInstance(fieldInfo.FieldType);
                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)datas;

                while (reader.Read())
                {
                    try
                    {
                        var key = reader.GetValue(keyColumn.Index);
                        var convertedKey = ConvertValue(key, keyType);
                        var record = ReadRecord(reader, schema);

                        if (dictionary.Contains(convertedKey))
                        {
                            Debug.LogError($"[TableData] {schema.TableName}: duplicated key({convertedKey})");
                            continue;
                        }

                        methodInfo.Invoke(datas, new[] { convertedKey, record });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TableData] {schema.TableName}: fail add depth({reader.Depth})/ ex({ex.Message})");
                        continue;
                    }
                }

                var instance = ScriptableObject.CreateInstance(tableType);
                fieldInfo.SetValue(instance, datas);

                AssetDatabase.CreateAsset(instance, Path.Combine(path, $"{schema.TableName}.asset"));
            }
        }
    }
}