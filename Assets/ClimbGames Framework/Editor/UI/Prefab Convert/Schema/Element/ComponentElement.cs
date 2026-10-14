using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ComponentElement : BindingElement
    {
        public ComponentElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        public override object GetFieldValue()
        {
            var value = Target.GetComponent(FieldType);
            if (value != null)
                return value;

            Debug.LogError($"[ComponentElement] not exist component({FieldType.Name}): target({Target.name})");
            return base.GetFieldValue();
        }
    }
}