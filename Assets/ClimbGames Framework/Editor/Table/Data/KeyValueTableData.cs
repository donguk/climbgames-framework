using System;
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
                var valueColumn = columns[1];

                var tableType = Type.GetType($"{schema.Namespace}.{schema.TableName}Table, Assembly-CSharp");
                var fieldInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var methodInfo = fieldInfo.FieldType.GetMethod("Add");

                var genericArguments = fieldInfo.FieldType.GetGenericArguments();
                var keyType = genericArguments[0];
                var valueType = genericArguments[1];

                var datas = Activator.CreateInstance(fieldInfo.FieldType);
                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)datas;

                while (reader.Read())
                {
                    try
                    {
                        var key = reader.GetValue(keyColumn.Index);
                        var value = reader.GetValue(valueColumn.Index);

                        var convertedKey = ConvertValue(key, keyType);
                        var convertedValue = ConvertValue(value, valueType);

                        if (dictionary.Contains(convertedKey))
                        {
                            Debug.LogError($"[TableData] {schema.TableName}: duplicated key({convertedKey})/ depth({reader.Depth})");
                            continue;
                        }

                        methodInfo.Invoke(datas, new[] { convertedKey, convertedValue });
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