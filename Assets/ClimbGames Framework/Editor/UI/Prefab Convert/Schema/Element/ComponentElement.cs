using System;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ComponentElement : BindingElement
    {
        private Component component;

        public override Type FieldType => component != null ? component.GetType() : typeof(Transform);

        public ComponentElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void OnInitialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.OnInitialize(parent, schema, depth);

            component = Target.GetComponent(base.FieldType);
            if (component == null)
            {
                var fallback = binding.Fallback;
                if (fallback != null)
                {
                    foreach (var type in fallback)
                    {
                        component = Target.GetComponent(type);
                        if (component != null)
                            break;
                    }
                }
            }
        }

        public override string GetFieldTypeName()
        {
            if (component != null)
                return GetTypeName(component.GetType());

            return base.GetFieldTypeName();
        }

        public override object GetFieldValue()
        {
            if (component != null)
                return component;

            Debug.LogError($"[ComponentElement] not exist component({FieldType.Name}): target({Target.name})");
            return base.GetFieldValue();
        }
    }
}