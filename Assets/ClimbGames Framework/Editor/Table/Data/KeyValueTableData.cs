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
        public KeyValueTableData(TableSchema schema) : base(schema)
        {
        }

        public override ClimbGames.Table CreateAsset(IExcelDataReader reader, string path)
        {
            var tableType = Type.GetType($"{schema.Namespace}.{schema.TableName}Table, Assembly-CSharp");
            var listInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfo = listInfo.FieldType.GetMethod("Add");

            var list = Activator.CreateInstance(listInfo.FieldType);
            var columns = schema.GetTableHeader().Columns;
            if (columns.Count > 1)
            {
                var keyColumn = columns[0];

                var dictionaryInfo = tableType.GetField("dictionary", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var keyType = dictionaryInfo.FieldType.GetGenericArguments()[0];


                HashSet<object> keyHash = new HashSet<object>();

                while (reader.Read())
                {
                    try
                    {
                        var key = reader.GetValue(keyColumn.Index);
                        var convertedKey = ConvertValue(key, keyType);
                        if (keyHash.Add(convertedKey) == false)
                        {
                            Debug.LogError($"[Tables] {schema.TableName}: duplicated key({convertedKey})/ depth({reader.Depth})");
                            continue;
                        }

                        var record = ReadRecord(reader, schema);
                        methodInfo.Invoke(list, new[] { record });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Tables] {schema.TableName} can not add data({reader.Depth}): {ex}");
                        continue;
                    }
                }
            }

            var tableAsset = ScriptableObject.CreateInstance(tableType);
            listInfo.SetValue(tableAsset, list);

            AssetDatabase.CreateAsset(tableAsset, Path.Combine(path, $"{schema.TableName}.asset"));
            return tableAsset as ClimbGames.Table;
        }
    }
}