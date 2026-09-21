using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public class TableSchema : Schema, ISchema
    {
        private static readonly Dictionary<string, SchemaType> TableTypes = new Dictionary<string, SchemaType>(StringComparer.OrdinalIgnoreCase)
        {
            ["@list"] = SchemaType.ListTable,

            ["@dictionary"] = SchemaType.DictionaryTable,
            ["@dic"] = SchemaType.DictionaryTable,

            ["@keyvalue"] = SchemaType.KeyValueTable,
        };

        public SchemaType SchemaType { get; private set; }
        public string TableName { get; private set; }
        public TableHeader Header { get; private set; }
        public EnumSchema EnumSchema { get; private set; }

        public TableSchema(string name)
        {
            SchemaType = SchemaType.None;
            TableName = Regex.Replace(name, @"\s+", "").ToPascalCaseName();

            Header = new TableHeader(this);
            EnumSchema = new EnumSchema();
        }

        public TableSchema(string name, EnumSchema enumSchema) : this(name)
        {
            EnumSchema = enumSchema;
        }

        public bool Resolve(IExcelDataReader reader)
        {
            if (Read(reader))
                Header.Resolve(reader);

            return Header.IsValid;
        }

        public bool Read(IExcelDataReader reader)
        {
            while (reader.Read())
            {
                if (SchemaType == SchemaType.None)
                    ReadSchemaType(reader);

                if (EnumSchema.Read(reader) == false)
                    continue;

                if (Header.Read(reader))
                    break;
            }

            if (SchemaType == SchemaType.None)
                SchemaType = Header.SchemaType;

            return Header.IsValid;
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