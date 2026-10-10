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
        public DictionaryTableData(TableSchema schema) : base(schema)
        {
        }

        public override ClimbGames.Table CreateAsset(IExcelDataReader reader, string path)
        {
            var tableType = Type.GetType($"{schema.Namespace}.{schema.TableName}Table, Assembly-CSharp");
            var listInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfo = listInfo.FieldType.GetMethod("Add");

            var list = Activator.CreateInstance(listInfo.FieldType);
            var keyColumn = schema.TableHeader.KeyColumn;
            if (keyColumn != null)
            {
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
                            Debug.LogError($"[Tables] {schema.TableName}: duplicated key({convertedKey})");
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