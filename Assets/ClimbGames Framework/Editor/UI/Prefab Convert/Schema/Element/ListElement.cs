using System;
using System.Collections.Generic;
using System.Text;
using ClimbGames.Editor.UI;
using UnityEngine;

namespace ClimbGames.Eidot.UI
{
    public class ListElement : BindingElement
    {
        public ListElement(GameObject target, Binding binding = null) : base(target, binding)
        {
        }

        public override void RetrieveFieldText(StringBuilder builder)
        {
            builder.AppendLine(GetFieldText("        ", true));
        }

        public override string GetFieldText(string indent, bool uniqueName)
        {
            Type fieldType = typeof(Transform);// binding.GetFieldType();

            string typeName = fieldType.FullName.TrimStart($"{schema.Namespace}.", "UnityEngine.UI.", "UnityEngine.", "TMPro.");
            string fieldName = FieldName;

            if (uniqueName)
                fieldName = schema.GetUniqueFieldName(this);

            return $"{indent}[SerializeField] private List<{typeName}> {fieldName};";
        }

        public override void RetrieveGroupChildren(ref List<BindingElement> list)
        {
            list.Add(this);
        }
    }
}