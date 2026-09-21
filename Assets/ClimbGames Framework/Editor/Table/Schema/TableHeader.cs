using System;
using System.Collections.Generic;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public class TableHeader
    {
        private TableSchema schema;
        private List<ColumnInfo> columns;
        private int keyIndex;
        private List<ColumnInfo> resolveColumns;

        public SchemaType SchemaType { get; private set; }
        public bool IsValid => columns.Count > 0;
        public IReadOnlyList<ColumnInfo> Columns => columns;
        public ColumnInfo KeyColumn => keyIndex < columns.Count ? columns[keyIndex] : null;
        public int ResolveCount => resolveColumns.Count;

        public TableHeader(TableSchema schema)
        {
            this.schema = schema;

            columns = new List<ColumnInfo>();
            resolveColumns = new List<ColumnInfo>();
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

                    if (ColumnInfo.TryParse(columnName, i, schema, out var column))
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

                        columns.Add(column);

                        if (string.IsNullOrEmpty(column.TypeName))
                            resolveColumns.Add(column);
                    }
                }
            }

            if (columns.Count > 0 && SchemaType == SchemaType.None)
                SchemaType = SchemaType.DictionaryTable; // default type

            return columns.Count > 0;
        }

        public bool Resolve(IExcelDataReader reader)
        {
            for (int i = 0; i < resolveColumns.Count;)
            {
                var colunm = resolveColumns[i];

                var fieldType = reader.GetFieldType(colunm.Index);
                if (fieldType != null)
                {
                    colunm.SetFieldType(fieldType);
                    resolveColumns.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            return ResolveCount <= 0;
        }
    }
}