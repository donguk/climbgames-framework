using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public enum TableType
    {
        None,
        List,
        Dictionary,
        KeyValue,
    }

    public class TableSchema : ISchema
    {
        private static readonly Dictionary<string, TableType> TableTypes = new Dictionary<string, TableType>(StringComparer.OrdinalIgnoreCase)
        {
            ["@list"] = TableType.List,

            ["@dictionary"] = TableType.Dictionary,
            ["@dic"] = TableType.Dictionary,

            ["@keyvalue"] = TableType.KeyValue,
        };
        private static Regex ColumnRegex = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_]*)(?:\[([^\]]+)\])?$");

        private List<ColumnInfo> columnList;
        private int keyColumnIndex;
        private Type cachedRecordType;
        private Dictionary<string, FieldInfo> cachedFieldInfos;

        public string Namespace { get; private set; }
        public string TableName { get; private set; }
        public TableType TableType { get; private set; }

        public bool IsValid => ColumnCount > 0;
        public int ColumnCount => columnList != null ? columnList.Count : 0;
        public IReadOnlyList<ColumnInfo> ColumnList => columnList;
        public ColumnInfo KeyColumn => keyColumnIndex < columnList.Count ? columnList[keyColumnIndex] : null;

        public TableSchema(string name)
        {
            Namespace = FrameworkEditorSettings.instance.ProjectNamesapce;
            if (string.IsNullOrEmpty(Namespace))
                Namespace = "ClimbGames";

            TableName = Regex.Replace(name, @"\s+", "").ToPascalCaseName();
            TableType = TableType.None;

            columnList = new List<ColumnInfo>();
            cachedFieldInfos = new Dictionary<string, FieldInfo>();
        }

        public bool Resolve(IExcelDataReader reader)
        {
            if (Read(reader))
            {
                using (new ListPoolScope<int>(out var list))
                {
                    for (int i = 0; i < columnList.Count; ++i)
                    {
                        if (columnList[i].FieldType == null)
                            list.Add(i);
                    }

                    while (list.Count > 0 && reader.Read())
                    {
                        for (int i = 0; i < list.Count;)
                        {
                            var colunm = columnList[list[i]];

                            var fieldType = reader.GetFieldType(colunm.Index);
                            if (fieldType != null && fieldType != typeof(object))
                            {
                                colunm.SetType(fieldType);
                                list.RemoveAt(i);
                            }
                            else
                            {
                                ++i;
                            }
                        }
                    }
                }
            }

            return IsValid;
        }

        public bool Read(IExcelDataReader reader)
        {
            while (reader.Read())
            {
                if (ReadColumn(reader))
                    break;
            }

            return IsValid;
        }

        bool ReadColumn(IExcelDataReader reader)
        {
            columnList.Clear();
            keyColumnIndex = 0;

            HashSet<string> nameHash = new HashSet<string>();
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                Type columnType = reader.GetFieldType(i);
                if (columnType != null && columnType == typeof(string))
                {
                    string columnName = reader.GetString(i);

                    // 테이블 타입 확인
                    if (TableType == TableType.None)
                    {
                        if (columnName.StartsWith("@") && TableTypes.TryGetValue(columnName, out var tableType))
                        {
                            TableType = tableType;
                            continue;
                        }
                    }

                    var match = ColumnRegex.Match(columnName.Trim());
                    if (match.Success)
                    {
                        string fieldName = match.Groups[1].Value;
                        if (nameHash.Add(fieldName) == false)
                        {
                            Debug.LogError($"[TableSchema] {TableName}: duplicated column({i})/ name({fieldName})");
                            continue;
                        }

                        var columnInfo = new ColumnInfo(i, fieldName);
                        columnList.Add(columnInfo);

                        if (match.Groups[2].Success)
                        {
                            string typeName = match.Groups[2].Value.Trim();
                            if (typeName.StartsWith("key:", StringComparison.OrdinalIgnoreCase))
                            {
                                TableType = TableType.Dictionary;
                                keyColumnIndex = columnList.Count - 1;
                                typeName = typeName["key:".Length..].Trim();
                            }

                            if (ColumnType.TryParse(TableName, i, typeName, out var fieldType) == false)
                                Debug.LogError($"[TableSchema] {TableName}: invalid type({typeName}) column({i})/ name({fieldName})");

                            columnInfo.SetType(fieldType);
                        }
                    }
                }
            }

            if (columnList.Count > 0 && TableType == TableType.None)
                TableType = TableType.Dictionary; // default

            return columnList.Count > 0;
        }

        public Type GetRecordType()
        {
            if (cachedRecordType == null)
            {
                switch (TableType)
                {
                    case TableType.List:
                    case TableType.Dictionary:
                        {
                            cachedRecordType = Type.GetType($"{Namespace}.{TableName}TableRecord, Assembly-CSharp");
                            break;
                        }
                    case TableType.KeyValue:
                        {
                            if (columnList.Count > 1)
                                cachedRecordType = columnList[1].FieldType;
                            break;
                        }
                }
            }

            return cachedRecordType;
        }

        public FieldInfo GetFieldInfo(string fieldName)
        {
            if (cachedFieldInfos.TryGetValue(fieldName, out var fieldInfo) == false)
            {
                var type = GetRecordType();
                if (type != null)
                    cachedFieldInfos[fieldName] = fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return fieldInfo;
        }
    }
}


