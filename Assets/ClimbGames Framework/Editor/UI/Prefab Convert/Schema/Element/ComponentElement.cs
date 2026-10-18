using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ComponentElement : BindingElement
    {
        private Component component;

        public ComponentElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void OnInitialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.OnInitialize(parent, schema, depth);

            component = Target.GetComponent(FieldType);
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