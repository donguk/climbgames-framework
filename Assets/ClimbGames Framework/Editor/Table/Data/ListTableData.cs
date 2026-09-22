using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExcelDataReader;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public class ListTableData : TableData
    {
        private TableSchema schema;

        public ListTableData(TableSchema schema)
        {
            this.schema = schema;
        }

        public override void CreateAsset(IExcelDataReader reader, string path)
        {
            var tableType = Type.GetType($"{schema.Namespace}.{schema.TableName}Table, Assembly-CSharp");
            var listInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfo = listInfo.FieldType.GetMethod("Add");

            var list = Activator.CreateInstance(listInfo.FieldType);
            while (reader.Read())
            {
                try
                {
                    var record = ReadRecord(reader, schema);
                    methodInfo.Invoke(list, new[] { record });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Tables] {schema.TableName} can not add data({reader.Depth}): {ex.Message}");
                    continue;
                }
            }

            var tableAsset = ScriptableObject.CreateInstance(tableType);
            listInfo.SetValue(tableAsset, list);

            AssetDatabase.CreateAsset(tableAsset, Path.Combine(path, $"{schema.TableName}.asset"));
        }
    }
}