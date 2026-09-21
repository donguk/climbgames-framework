using System;
using System.Collections.Generic;
using ExcelDataReader;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ClimbGames.Editor.Table
{
    public interface ITableData
    {
        void CreateAsset(IExcelDataReader reader, string path);
    }

    public class TableData : ITableData
    {
        public static ITableData Get(TableSchema schema)
        {
            switch (schema.SchemaType)
            {
                case SchemaType.ListTable: return new ListTableData(schema);
                case SchemaType.DictionaryTable: return new DictionaryTableData(schema);
                case SchemaType.KeyValueTable: return new KeyValueTableData(schema);
            }

            return new TableData();
        }

        private Dictionary<string, FieldInfo> fieldInfos;

        public virtual void CreateAsset(IExcelDataReader reader, string path)
        {
            throw new NotImplementedException();
        }

        protected object ReadRecord(IExcelDataReader reader, TableSchema schema)
        {
            if (fieldInfos == null)
                fieldInfos = new Dictionary<string, FieldInfo>();

            var recordType = Type.GetType($"{schema.Namespace}.{schema.TableName}TableRecord, Assembly-CSharp");
            var data = Activator.CreateInstance(recordType);

            var columns = schema.Header.Columns;
            for (int i = 0; i < columns.Count; ++i)
            {
                var value = reader.GetValue(columns[i].Index);
                string fieldName = columns[i].FieldName;

                if (fieldInfos.TryGetValue(fieldName, out var fieldInfo) == false)
                    fieldInfos[fieldName] = fieldInfo = recordType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsGenericType)
                {
                    if (fieldType.GetGenericTypeDefinition() == typeof(List<>))
                        fieldInfo.SetValue(data, ReadListValue(fieldType, value));
                }
                else
                {
                    object convertedValue = ConvertValue(value, fieldType);
                    fieldInfo.SetValue(data, convertedValue);
                }
            }

            return data;
        }

        object ReadListValue(Type listType, object value)
        {
            if (value == null)
                return null;

            var argumentType = listType.GetGenericArguments()[0];
            var methodInfo = listType.GetMethod("Add");
            var list = Activator.CreateInstance(listType);

            string[] values = value.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < values.Length; ++i)
            {
                var data = Convert.ChangeType(values[i].Trim(), argumentType);
                methodInfo.Invoke(list, new[] { data });
            }

            return list;
        }

        protected object ConvertValue(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (targetType.IsInstanceOfType(value))
                return value;

            if (targetType.IsEnum)
            {
                try
                {
                    if (value is string text)
                        return Enum.Parse(targetType, text, ignoreCase: true);

                    return Enum.ToObject(targetType, value);
                }
                catch (ArgumentException)
                {
                    return Activator.CreateInstance(targetType);
                }
            }

            return Convert.ChangeType(value, targetType);
        }
    }
}