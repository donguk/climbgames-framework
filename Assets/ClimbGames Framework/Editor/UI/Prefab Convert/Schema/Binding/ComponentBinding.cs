using System;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ComponentBinding : Binding
    {
        private Type type;

        public ComponentBinding(Type type, string prefix) : base(prefix)
        {
            this.type = type;
        }

        public ComponentBinding(Schema schema) : base(schema.Prefix)
        {
            type = Type.GetType(schema.typeName);
        }

        public override BindingElement CreateElement(GameObject target)
        {
            return new ComponentElement(target, this);
        }

        public override Type GetFieldType()
        {
            if (type == null)
                Debug.LogError($"[ComponentBinding] {prefix.ToString()} type is null.");

            return type;
        }
    }
}