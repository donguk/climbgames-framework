using System.Collections.Generic;
using System.Linq;

namespace ClimbGames.Editor.UI
{
    public class GroupDefinition
    {
        public string name;
        private Dictionary<string/*fieldName*/, BindingElement> elements;

        public IReadOnlyList<BindingElement> Elements => elements.Values.ToList();

        public GroupDefinition(string name)
        {
            this.name = name;
            elements = new Dictionary<string, BindingElement>();
        }

        public void AddElements(List<BindingElement> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                string fieldName = list[i].FieldName;
                if (elements.ContainsKey(fieldName) == false)
                    elements[fieldName] = list[i];
                else
                    Debug.LogError($"[GroupDefinition] {name} Group has duplicated fieldName: {fieldName}");
            }
        }
    }

    public class GroupSchema : Schema
    {
        public override string ScriptName => "GropuSchema";

        private Dictionary<string, GroupDefinition> definitions;

        public IReadOnlyList<GroupDefinition> Definitions => definitions.Values.ToList();

        public GroupSchema()
        {
            definitions = new Dictionary<string, GroupDefinition>();
        }

        public void AddDefinition(string groupName, List<BindingElement> elements)
        {
            if (definitions.TryGetValue(groupName, out var definition) == false)
                definitions[groupName] = definition = new GroupDefinition(groupName);

            definition.AddElements(elements);
        }
    }
}