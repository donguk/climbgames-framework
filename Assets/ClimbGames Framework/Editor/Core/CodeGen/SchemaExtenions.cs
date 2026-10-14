using System;
using System.Collections.Generic;
using System.Text;

namespace ClimbGames.Editor
{
    public static class SchemaExtenions
    {
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

        private static readonly HashSet<string> Keywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte",
            "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern",
            "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new",
            "null", "object", "operator", "out", "override",
            "params", "private", "protected", "public", "readonly",
            "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "stackalloc", "static", "string", "struct", "switch",
            "this", "throw", "true", "try", "typeof", "uint",
            "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while"
        };

        public static string ToValidFieldName(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var builder = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (builder.Length == 0)
                {
                    if (char.IsLetter(c) || c == '_')
                        builder.Append(c);
                    else if (char.IsDigit(c))
                    {
                        builder.Append('_');
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append('_');
                    }
                }
                else
                {
                    builder.Append(
                        char.IsLetterOrDigit(c) || c == '_'
                            ? c
                            : '_'
                    );
                }
            }

            string fieldName = builder.ToString();

            // C# 예약어 처리
            if (Keywords.Contains(fieldName))
                fieldName = "_" + fieldName;

            return fieldName;
        }
    }
}