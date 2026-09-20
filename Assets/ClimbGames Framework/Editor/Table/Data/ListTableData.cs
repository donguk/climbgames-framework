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
            var fieldInfo = tableType.GetField("datas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfo = fieldInfo.FieldType.GetMethod("Add");

            var datas = Activator.CreateInstance(fieldInfo.FieldType);
            while (reader.Read())
            {
                try
                {
                    var record = ReadRecord(reader, schema);
                    methodInfo.Invoke(datas, new[] { record });
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