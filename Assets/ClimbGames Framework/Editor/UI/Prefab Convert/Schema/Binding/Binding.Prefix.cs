using System;

namespace ClimbGames.Editor.UI
{
    public partial class Binding
    {
        [Serializable]
        public class Prefix
        {
            public string[] values;

            public Prefix(string value = default)
            {
                if (string.IsNullOrEmpty(value))
                    values = new string[] { };
                else
                    values = new string[] { value };
            }

            public bool Match(string name)
            {
                foreach (var value in values)
                {
                    if (name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }

            public override string ToString()
            {
                string text = string.Empty;
                for (int i = 0; i < values.Length; ++i)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += values[i];
                }

                return $"prefix: [{text}]";
            }
        }
    }
}