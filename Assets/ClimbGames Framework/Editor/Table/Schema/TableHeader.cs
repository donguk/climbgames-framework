using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public class TableHeader
    {
        private static Regex ColumnRegex = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_]*)(?:\[([^\]]+)\])?$");
        private static Regex KeyRegex = new Regex(@"^(?i:key)(?:<[^>]+>)?$");

        private TableSchema schema;
        private List<ColumnInfo> columns;
        private int keyIndex;

        public SchemaType SchemaType { get; private set; }
        public bool IsValid => columns.Count > 0;
        public IReadOnlyList<ColumnInfo> Columns => columns;
        public ColumnInfo KeyColumn => keyIndex < columns.Count ? columns[keyIndex] : null;

        public TableHeader(TableSchema schema)
        {
            this.schema = schema;
            columns = new List<ColumnInfo>();
        }

        public bool Read(IExcelDataReader reader)
        {
            keyIndex = 0;
            columns.Clear();

            HashSet<string> nameHash = new HashSet<string>();
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                Type type = reader.GetFieldType(i);
                if (type != null && type == typeof(string))
                {
                    string columnName = reader.GetString(i);

                    if (ColumnInfo.TryParse(columnName, schema, out var column))
                    {
                        if (nameHash.Add(column.FieldName) == false)
                        {
                            Debug.LogError($"[TableHeader] {schema.TableName}: duplicated column({i})/ name({column.FieldName})");
                            continue;
                        }

                        if (column.IsKey)
                        {
                            SchemaType = SchemaType.DictionaryTable;
                            keyIndex = columns.Count;
                        }

                        column.Index = i;
                        columns.Add(column);
                    }
                }
            }

            if (columns.Count > 0 && SchemaType == SchemaType.None)
                SchemaType = SchemaType.DictionaryTable; // default type

            return columns.Count > 0;
        }

        public void Resolve(IExcelDataReader reader)
        {
            using (new ListPoolScope<int>(out var list))
            {
                for (int i = 0; i < columns.Count; ++i)
                {
                    if (string.IsNullOrEmpty(columns[i].TypeName))
                        list.Add(i);
                }

                while (list.Count > 0 && reader.Read())
                {
                    for (int i = 0; i < list.Count;)
                    {
                        var colunm = columns[list[i]];

                        var fieldType = reader.GetFieldType(colunm.Index);
                        if (fieldType != null && fieldType != typeof(object))
                        {
                            colunm.SetFieldType(fieldType);
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
    }
}