using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ComponentElement : BindingElement
    {
        public ComponentElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        public override void RetrieveFieldText(StringBuilder builder)
        {
            builder.AppendLine(GetFieldText("        ", true));
            base.RetrieveFieldText(builder);
        }

        public override string GetFieldText(string indent, bool uniqueName)
        {
            Type fieldType = binding.GetFieldType();

            string typeName = fieldType.FullName.TrimStart($"{schema.Namespace}.", "UnityEngine.UI.", "UnityEngine.", "TMPro.");
            string fieldName = FieldName;

            if (uniqueName)
                fieldName = schema.GetUniqueFieldName(this);

            return $"{indent}[SerializeField] private {typeName} {fieldName};";
        }

        public override void RetrieveGroupChildren(ref List<BindingElement> list)
        {
            list.Add(this);
            base.RetrieveGroupChildren(ref list);
        }
    }
}