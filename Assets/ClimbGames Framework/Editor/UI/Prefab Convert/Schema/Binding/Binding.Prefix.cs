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
                values = new string[] { value };
            }

            public Prefix(string[] values)
            {
                this.values = values;
            }

            public bool Match(string name)
            {
                if (values == null)
                    return false;

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
                if (values != null)
                {
                    for (int i = 0; i < values.Length; ++i)
                    {
                        if (text.Length > 0)
                            text += ", ";
                        text += values[i];
                    }
                }

                return $"prefix: [{text}]";
            }
        }
    }
}