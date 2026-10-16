using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClimbGames.UI;
using ClimbGames.Eidot.UI;
using System.Collections.Generic;
using System.Linq;

namespace ClimbGames.Editor.UI
{
    public enum BindingType
    {
        None,
        Component,
        Group,
        List,
        UIBase,
    }

    public partial class Binding
    {
        private BindingType bindingType;
        private Prefix prefix;

        public BindingType Type => bindingType;
        public Type FieldType { get; private set; }
        public string MatchValue { get; private set; }

        public Binding(BindingType type, string prefix, Type fieldType)
        {
            bindingType = type;
            this.prefix = new Prefix(prefix);
            FieldType = fieldType;
        }

        public Binding(BindingType type, string prefix) : this(type, prefix, typeof(Transform))
        {
        }

        public Binding(Schema schema)
        {
            string[] values = schema.prefix.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            prefix = new Prefix(values);
            bindingType = schema.bindingType;

            if (string.IsNullOrEmpty(schema.typeName) == false)
                FieldType = System.Type.GetType(schema.typeName);
        }

        public Binding(BindingType type, Prefix prefix, Type fieldType)
        {
            bindingType = type;
            this.prefix = prefix;
            FieldType = fieldType;
        }

        public Binding(BindingType type, Prefix prefix) : this(type, prefix, typeof(Transform))
        {
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

        public BindingElement CreateElement(GameObject target)
        {
            return bindingType switch
            {
                BindingType.Component => new ComponentElement(target, this),
                BindingType.Group => new GroupElement(target, this),
                BindingType.List => new ListElement(target, this),
                BindingType.UIBase => new UIBaseElement(target, this),
                _ => new BindingElement(target, this),
            };
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
