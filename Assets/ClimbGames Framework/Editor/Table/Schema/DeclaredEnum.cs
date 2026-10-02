using System;
using System.Text.RegularExpressions;

namespace ClimbGames.Editor.Table
{
    public class DeclaredEnum
    {
        public static readonly Regex EnumRegex = new Regex(@"\benum\s+([A-Za-z_][A-Za-z0-9_]*)\b");

        public Type enumType;
        public EnumDefinition definition;

        public DeclaredEnum(Type type)
        {
            enumType = type;
            definition = new EnumDefinition(enumType.Name);

            string[] names = Enum.GetNames(enumType);
            foreach (var name in names)
                definition.AddValue(name);
        }
    }
}