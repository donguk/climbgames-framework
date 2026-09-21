using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClimbGames.Editor.Table
{
    public class EnumDefinition
    {
        private static readonly Regex EnumRegex = new Regex(@"^\[enum:([A-Za-z_][A-Za-z0-9_]*)\]$");
        public static readonly Regex NameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

        public string Name { get; private set; }
        public bool IsDeclaration { get; private set; }

        private HashSet<string> values;
        public IReadOnlyList<string> Values => values.ToList();

        public EnumDefinition(string name, bool isDeclaration = false)
        {
            Name = name;
            IsDeclaration = isDeclaration;

            values = new HashSet<string>();
        }

        public void AddValue(string value)
        {
            values.Add(value);
        }

        public void Merge(EnumDefinition other)
        {
            foreach (var value in other.values)
                values.Add(value);
        }

        public static bool TryParse(string value, out EnumDefinition declaration)
        {
            declaration = null;
            var match = EnumRegex.Match(value.Trim());

            if (match.Success)
                declaration = new EnumDefinition(match.Groups[1].Value, true);

            return declaration != null;
        }
    }
}