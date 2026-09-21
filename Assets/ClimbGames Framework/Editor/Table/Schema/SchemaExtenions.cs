using System;
using System.Collections.Generic;

namespace ClimbGames.Editor.Table
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
    }
}