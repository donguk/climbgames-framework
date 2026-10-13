using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClimbGames.UI;
using ClimbGames.Eidot.UI;

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
        public static readonly Binding Default = new Binding(BindingType.None, string.Empty);

        [Serializable]
        public class Schema
        {
            public Prefix Prefix;
            public string typeName;
        }

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





        private static Binding[] bindings = new Binding[]
        {
            new Binding(BindingType.Component, "trs_", typeof(Transform)),
            new Binding(BindingType.Component, "txt_", typeof(TMP_Text)),
            new Binding(BindingType.Component, "img_", typeof(Image)),

            new Binding(BindingType.Group, "grp_"),
            new Binding(BindingType.List, "list_"),

            new Binding(BindingType.UIBase, "ui_", typeof(UIBase)),
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
