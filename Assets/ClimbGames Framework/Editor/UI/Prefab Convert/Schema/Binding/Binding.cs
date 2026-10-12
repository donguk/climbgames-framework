using System;
using ClimbGames.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClimbGames.Editor.UI
{
    public abstract class Binding
    {
        [Serializable]
        public class Prefix
        {
            public string[] values;

            public Prefix(string value)
            {
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

        [Serializable]
        public class Schema
        {
            public Prefix Prefix;
            public string typeName;
        }




        protected Prefix prefix;
        public string MatchValue { get; private set; }

        public Binding(string prefix)
        {
            this.prefix = new Prefix(prefix);
        }

        public Binding(Prefix prefix)
        {
            this.prefix = prefix;
        }

        public virtual bool Match(string name)
        {
            if (prefix.Match(name))
            {
                MatchValue = name.TrimStart(prefix.values);
                return true;
            }
            return false;
        }

        public abstract BindingElement CreateElement(GameObject target);

        public virtual Type GetFieldType() { return typeof(Transform); }




        private static Binding[] bindings = new Binding[]
        {
            new ComponentBinding(typeof(Transform), "trs_"),
            new ComponentBinding(typeof(TMP_Text), "txt_"),
            new ComponentBinding(typeof(Image), "img_"),

            new ComponentBinding(typeof(UIBase), "ui_"),

            new GroupBinding("grp_"),
            new ListBinding("list_"),
        };

        public static bool TryGetMatch(string name, out Binding result)
        {
            result = null;
            foreach (var match in bindings)
            {
                if (match.Match(name))
                {
                    result = match;
                    break;
                }
            }

            return result != null;
        }
    }
}
