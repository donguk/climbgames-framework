using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ClimbGames.Editor.UI
{
    public partial class Binding
    {
        private Prefix prefix;
        private List<Type> fallback;
        private ElementProvider elementProvider;

        public Type FieldType { get; private set; }
        public string MatchValue { get; private set; }
        public IReadOnlyList<Type> Fallback => fallback;
        public SchemaOption Options { get; private set; }

        public Binding(string prefix, Type fieldType)
        {
            this.prefix = new Prefix(prefix);
            FieldType = fieldType;
        }

        public Binding(Prefix prefix, Type fieldType)
        {
            this.prefix = prefix;
            FieldType = fieldType;
        }

        public Binding(Prefix prefix) : this(prefix, typeof(Transform))
        {
        }

        public Binding(Schema schema)
        {
            string[] values = schema.prefix.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            prefix = new Prefix(values);

            if (string.IsNullOrEmpty(schema.typeName) == false)
                FieldType = System.Type.GetType(schema.typeName);

            if ((schema.options & SchemaOption.Fallback) != 0)
            {
                fallback = new List<Type>();
                if (schema.fallback != null)
                {
                    for (int i = 0; i < schema.fallback.Length; ++i)
                    {
                        if (string.IsNullOrEmpty(schema.fallback[i]) == false)
                            fallback.Add(System.Type.GetType(schema.fallback[i]));
                    }
                }
            }

            Options = schema.options;
            elementProvider = schema.GetElementProvider(this);
        }

        public bool Match(string name)
        {
            if (prefix.Match(name))
            {
                MatchValue = name.TrimStart(prefix.values);
                return true;
            }
            return false;
        }

        public BindingElement CreateElement(GameObject target)
        {
            if (elementProvider != null)
                return elementProvider.CreateElement(target);

            return new BindingElement(target, this);
        }

        private static List<Binding> bindings;

        public static void Initialize(Schema[] schemas)
        {
            if (bindings == null)
                bindings = new List<Binding>();

            bindings.Clear();

            foreach (var schema in schemas)
                bindings.Add(new Binding(schema));
        }

        public static bool TryGetMatch(string name, out Binding result)
        {
            result = null;

            if (bindings == null)
                return false;

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
