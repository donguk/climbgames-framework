using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ClimbGames.Editor.Table
{
    public class ColumnInfo
    {
        private static readonly Regex ColumnRegex = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_]*)(?:\[([^\]]+)\])?$");
        private static readonly Regex KeyRegex = new Regex(@"^(?i:key)(?:<([^>]+)>)?$");
        private static readonly Regex ListRegex = new Regex(@"^(?i:list)(?:<([^>]+)>)?$");
        private static readonly Regex EnumRegex = new Regex(@"^(?i:enum):([A-Za-z_][A-Za-z0-9_]*)$");

        public int Index { get; set; }
        public string FieldName { get; private set; }
        public string PropertyName { get; private set; }
        public string TypeName { get; private set; }

        public bool IsKey { get; private set; }
        public bool IsList { get; private set; }
        public bool IsEnum { get; private set; }

        public ColumnInfo(string name)
        {
            FieldName = name.ToCamelCaseName();
            PropertyName = name.ToPascalCaseName();
        }

        public static bool TryParse(string value, TableSchema schema, out ColumnInfo column)
        {
            column = Parse(value, schema);
            return column != null;
        }

        public static ColumnInfo Parse(string value, TableSchema schema)
        {
            var match = ColumnRegex.Match(value);
            if (match.Success)
            {
                string fieldName = match.Groups[1].Value;
                var column = new ColumnInfo(fieldName);

                if (match.Groups[2].Success)
                {
                    string typeName = match.Groups[2].Value;

                    match = KeyRegex.Match(typeName.Trim());
                    if (match.Success)
                    {
                        column.IsKey = true;
                        typeName = match.Groups[1].Value;
                    }

                    if (string.IsNullOrEmpty(typeName) == false)
                    {
                        if (column.ParseType(typeName.Trim(), schema) == false)
                            Debug.LogError($"[TableHeader] {schema.TableName}: invalid column({column.FieldName})/ type({typeName})");
                    }
                }

                return column;
            }

            return null;
        }

        bool ParseType(string value, TableSchema schema)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            value = value.Trim();
            switch (value)
            {
                case "bool":
                case "string":
                case "short":
                case "int":
                case "long":
                case "ushort":
                case "uint":
                case "ulong":
                case "float":
                case "double":
                    {
                        TypeName = value;
                        break;
                    }
                default:
                    {
                        var match = ListRegex.Match(value);
                        if (match.Success)
                        {
                            IsList = true;
                            if (ParseType(match.Groups[1].Value, schema) == false)
                                TypeName = "string";
                        }
                        else
                        {
                            match = EnumRegex.Match(value);
                            if (match.Success)
                            {
                                IsEnum = true;
                                TypeName = match.Groups[1].Value;

                                // enum 추가
                                schema.EnumSchema.AddDefinition(TypeName);
                            }
                        }
                        break;
                    }
            }

            return string.IsNullOrEmpty(TypeName) == false;
        }

        public void SetFieldType(Type type)
        {
            TypeName = Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.Byte => "byte",
                TypeCode.Char => "char",
                TypeCode.Decimal => "decimal",
                TypeCode.Double => "double",
                TypeCode.Int16 => "short",
                TypeCode.Int32 => "int",
                TypeCode.Int64 => "long",
                TypeCode.SByte => "sbyte",
                TypeCode.Single => "float",
                TypeCode.String => "string",
                TypeCode.UInt16 => "ushort",
                TypeCode.UInt32 => "uint",
                TypeCode.UInt64 => "ulong",
                _ => "string"
            };
        }

        public string GetTypeCodeName(TableSchema schema)
        {
            if (string.IsNullOrEmpty(TypeName))
                return "string";

            if (IsList)
                return $"List<{(IsEnum ? schema.EnumSchema.GetCodeName(TypeName) : TypeName)}>";

            if (IsEnum)
                return schema.EnumSchema.GetCodeName(TypeName);

            return TypeName;
        }
    }
}