using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class GroupElement : BindingElement
    {
        public string GroupName { get; private set; }

        public GroupElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void Initialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.Initialize(parent, schema, depth);

            GroupName = binding.MatchValue;
        }

        public override void RetrieveFieldText(StringBuilder builder)
        {
            builder.AppendLine(GetFieldText("        ", true));
        }

        public override string GetFieldText(string indent, bool uniqueName)
        {
            string typeName = $"{GroupName}";
            string fieldName = FieldName;

            if (uniqueName)
                fieldName = schema.GetUniqueFieldName(this);

            return $"{indent}[SerializeField] private {typeName} {fieldName};";
        }

        public override void RetrieveGroup(GroupSchema groupSchema)
        {
            using (new ListPoolScope<BindingElement>(out var list))
            {
                // 하위 element 수집
                if (children != null)
                {
                    for (int i = 0; i < children.Count; ++i)
                        children[i].RetrieveGroupChildren(ref list);
                }

                groupSchema.AddDefinition(GroupName, list);
            }

            // for nested group
            base.RetrieveGroup(groupSchema);
        }

        public override void RetrieveGroupChildren(ref List<BindingElement> list)
        {
            list.Add(this);
        }
    }
}