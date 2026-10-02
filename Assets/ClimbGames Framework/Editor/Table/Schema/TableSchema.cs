using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public class TableSchema : Schema
    {
        private static readonly Dictionary<string, SchemaType> TableTypes = new Dictionary<string, SchemaType>(StringComparer.OrdinalIgnoreCase)
        {
            ["@list"] = SchemaType.ListTable,

            ["@dictionary"] = SchemaType.DictionaryTable,
            ["@dic"] = SchemaType.DictionaryTable,

            ["@keyvalue"] = SchemaType.KeyValueTable,
        };

        private TableHeader tableHeader;
        private EnumSchema enumSchema;

        public TableSchema(string name)
        {
            SchemaType = SchemaType.None;
            TableName = Regex.Replace(name, @"\s+", "").ToPascalCaseName();

            tableHeader = new TableHeader(this);
            enumSchema = new EnumSchema();
        }

        public TableSchema(string name, EnumSchema enumSchema) : this(name)
        {
            this.enumSchema = enumSchema;
        }

        public override EnumSchema GetEnumSchema() => enumSchema;
        public override TableHeader GetTableHeader() => tableHeader;

        public bool Resolve(IExcelDataReader reader)
        {
            if (Read(reader))
            {
                while (reader.Read())
                {
                    if (tableHeader.Resolve(reader))
                    {
                        if (TableEditorSettings.ReadHeaderEnumValues == false)
                            break;
                    }

                    enumSchema.ReadTableValue(reader);
                }
            }

            return tableHeader.IsValid;
        }

        public bool Read(IExcelDataReader reader)
        {
            while (reader.Read())
            {
                if (SchemaType == SchemaType.None)
                    ReadSchemaType(reader);

                if (enumSchema.ReadDefinition(reader) == false)
                    continue;

                if (tableHeader.Read(reader))
                    break;
            }

            if (SchemaType == SchemaType.None)
                SchemaType = tableHeader.SchemaType;

            return tableHeader.IsValid;
        }

        void ReadSchemaType(IExcelDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                Type fieldType = reader.GetFieldType(i);
                if (fieldType != null && fieldType == typeof(string))
                {
                    string columnName = reader.GetString(i);
                    if (columnName.StartsWith("@") && TableTypes.TryGetValue(columnName, out var schemaType))
                    {
                        SchemaType = schemaType;
                        break;
                    }
                }
            }
        }
    }
}