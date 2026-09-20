using System;
using System.Collections.Generic;

namespace ClimbGames.Editor.Table
{
    public static class ColumnType
    {
        private static Dictionary<string, Type> cachedEnumTypes;

        private static bool TryGetEnumType(string enumName, out Type enumType)
        {
            if (cachedEnumTypes == null)
            {
                cachedEnumTypes = new Dictionary<string, Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsEnum)
                            continue;

                        cachedEnumTypes[type.Name] = type;
                    }
                }
            }

            return cachedEnumTypes.TryGetValue(enumName, out enumType);
        }

        public static bool TryParse(string tableName, int columnIndex, string typeName, out Type type)
        {
            type = Parse(tableName, columnIndex, typeName);
            return type != null;
        }

        public static Type Parse(string tableName, int columnIndex, string typeName)
        {
            switch (typeName)
            {
                case "string": return typeof(string);
                case "int": return typeof(int);
                case "float": return typeof(float);
                case "bool": return typeof(bool);
                case "long": return typeof(long);
                case "double": return typeof(double);
                default:
                    {
                        if (typeName.StartsWith("enum:", StringComparison.OrdinalIgnoreCase))
                        {
                            typeName = typeName["enum:".Length..].Trim();
                            if (TryGetEnumType(typeName, out var enumType) == false)
                                Debug.LogError($"[TableSchema] {tableName}: can not find enum type: {typeName}");

                            return enumType;
                        }
                        else if (typeName.StartsWith("list:", StringComparison.OrdinalIgnoreCase))
                        {
                            typeName = typeName["list:".Length..].Trim();
                            return typeof(List<>).MakeGenericType(Parse(tableName, columnIndex, typeName));
                        }

                        break;
                    }
            }

            return null;
        }

        public static string ToCodeName(this Type type, string @namespace = "")
        {
            if (type == null)
                return "string"; // default

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var argumentType = type.GetGenericArguments()[0];
                    return $"List<{argumentType.ToCodeName()}>";
                }
            }
            else if (type.IsEnum)
            {
                string fullName = type.FullName;
                if (fullName.StartsWith($"{@namespace}."))
                    fullName = fullName[$"{@namespace}.".Length..];

                return fullName;
            }

            return Type.GetTypeCode(type) switch
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
                _ => type.Name
            };
        }

        public static string ToCamelCaseName(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return char.ToLowerInvariant(name[0]) + name[1..];
        }

        public static string ToPascalCaseName(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return char.ToUpperInvariant(name[0]) + name[1..];
        }
    }
}