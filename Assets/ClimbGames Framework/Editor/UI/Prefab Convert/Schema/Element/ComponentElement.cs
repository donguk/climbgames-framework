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
            var value = target.GetComponent(FieldType);
            if (value != null)
                return value;

            Debug.LogError($"[ComponentElement] not exist component({FieldType.Name}): target({target.name})");
            return base.GetFieldValue();
        }
    }
}