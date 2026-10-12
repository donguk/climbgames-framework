using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class PrefabSchema : Schema
    {
        private BindingElement entry;
        private Dictionary<string, int> fieldNameCount;


        public string PrefabName { get; private set; }
        public override string ScriptName => PrefabName;
        public GroupSchema GroupSchema { get; private set; }

        public PrefabSchema(string name)
        {
            PrefabName = name;
            fieldNameCount = new Dictionary<string, int>();
            GroupSchema = new GroupSchema();
        }

        public void Read(Transform root)
        {
            fieldNameCount.Clear();

            entry = new BindingElement(root.gameObject, this);
            entry.SearchElements();

            // 그룹 정의 수집
            entry.RetrieveGroup(GroupSchema);
        }

        public string GetUniqueFieldName(BindingElement element)
        {
            string fieldName = element.FieldName;

            bool isDuplicated = fieldNameCount.TryGetValue(fieldName, out var duplicatedCount);
            while (isDuplicated)
            {
                BindingElement parent = element.Parent;
                if (parent == null || string.IsNullOrEmpty(parent.FieldName))
                    break;

                fieldName = $"{parent.FieldName}_{fieldName}";
                isDuplicated = fieldNameCount.TryGetValue(fieldName, out duplicatedCount);
                element = parent;
            }

            fieldNameCount[fieldName] = duplicatedCount + 1;

            if (isDuplicated)
                fieldName += $"_{duplicatedCount}";

            return fieldName;
        }

        public string GetFieldText()
        {
            if (entry == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            entry.RetrieveFieldText(builder);

            return builder.ToString();
        }
    }
}