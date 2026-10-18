using ClimbGames.Editor.UI;
using ClimbGames.UI;
using UnityEngine;

namespace ClimbGames
{
    public class UIBaseElement : BindingElement
    {
        private Component uiBase;

        public UIBaseElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void SearchElements(Transform transform, int depth)
        {
            // 자식을 찾지 않음.
        }

        protected override void OnInitialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.OnInitialize(parent, schema, depth);

            uiBase = Target.GetComponent(FieldType);
        }

        public override string GetFieldTypeName()
        {
            if (uiBase != null)
                return GetTypeName(uiBase.GetType());

            return base.GetFieldTypeName();
        }

        public override object GetFieldValue()
        {
            if (uiBase != null)
                return uiBase;

            Debug.LogError($"[UIBaseElement] not exist component({FieldType.Name}): target({Target.name})");
            return base.GetFieldValue();
        }
    }
}