using System;
using System.Collections;
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

        private EnumSchema enumSchema;

        public int Index { get; private set; }
        public string FieldName { get; private set; }
        public string PropertyName { get; private set; }
        public string TypeName { get; private set; }

        public bool IsKey { get; private set; }
        public bool IsList { get; private set; }
        public bool IsEnum { get; private set; }

        public ColumnInfo(int index, string name)
        {
            Index = index;
            FieldName = name.ToCamelCaseName();
            PropertyName = name.ToPascalCaseName();
        }

        public static bool TryParse(string value, int columnIndex, TableSchema schema, out ColumnInfo column)
        {
            column = Parse(value, columnIndex, schema);
            return column != null;
        }

        public static ColumnInfo Parse(string value, int columnIndex, TableSchema schema)
        {
            var match = ColumnRegex.Match(value);
            if (match.Success)
            {
                string fieldName = match.Groups[1].Value;
                var column = new ColumnInfo(columnIndex, fieldName)
                {
                    // enum 스키마 저장
                    enumSchema = schema.GetEnumSchema()
                };

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
                        if (column.ParseType(typeName.Trim()) == false)
                            Debug.LogError($"[TableHeader] {schema.TableName}: invalid column({column.FieldName})/ type({typeName})");
                    }
                }

                return column;
            }

            return null;
        }

        bool ParseType(string value)
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
                            if (ParseType(match.Groups[1].Value) == false)
                                TypeName = "string";
                        }
                        else
                        {
                            match = EnumRegex.Match(value);
                            if (match.Success)
                            {
                                IsEnum = true;
                                TypeName = match.Groups[1].Value;

                                // 테이블 헤더 enum 추가
                                enumSchema.AddTableHeader(TypeName, Index);
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
                TypeCode.Double => "double",
                TypeCode.Int16 => "short",
                TypeCode.Int32 => "int",
                TypeCode.Int64 => "long",
                TypeCode.Single => "float",
                TypeCode.String => "string",
                TypeCode.UInt16 => "ushort",
                TypeCode.UInt32 => "uint",
                TypeCode.UInt64 => "ulong",
                _ => "string"
            };
        }

        public TypeCode GetTypeCode()
        {
            return TypeName switch
            {
                "bool" => TypeCode.Boolean,
                "string" => TypeCode.String,
                "short" => TypeCode.UInt16,
                "int" => TypeCode.Int32,
                "long" => TypeCode.Int64,
                "ushort" => TypeCode.UInt16,
                "uint" => TypeCode.UInt32,
                "ulong" => TypeCode.UInt64,
                "float" => TypeCode.Single,
                "double" => TypeCode.Double,
                _ => TypeCode.String
            };
        }

        public string GetTypeCodeName()
        {
            if (string.IsNullOrEmpty(TypeName))
                return "string";

            if (IsList)
                return $"List<{(IsEnum ? enumSchema.GetTypeCodeName(TypeName) : TypeName)}>";

            if (IsEnum)
                return enumSchema.GetTypeCodeName(TypeName);

            return TypeName;
        }

        public string GetCastingText(bool assignTo)
        {
            if (IsEnum)
                return assignTo ? "(int)" : $"({enumSchema.GetTypeCodeName(TypeName)})";

            return string.Empty;
        }

        public string GetWriteText()
        {
            string text = FieldName;

            if (IsList)
                text += "[i]";

            switch (TypeName)
            {
                case "string": text += " ?? string.Empty"; break;
            }

            return text;
        }

        public string GetReadText()
        {
            if (IsEnum)
                return TypeCode.Int32.ToString();

            return GetTypeCode().ToString();
        }
    }
}