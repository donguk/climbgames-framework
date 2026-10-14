using System.Collections.Generic;
using System.Linq;

namespace ClimbGames.Editor.UI
{
    public class GroupDefinition
    {
        private Dictionary<string/*fieldName*/, string/*fieldType*/> elements;
        private UniqueNameGenerator nameGenerator;
        private Dictionary<string, string> notAssignedElements;

        public string Name { get; private set; }
        public int Depth { get; private set; }
        public IReadOnlyList<(string fieldName, string fieldType)> Elements => elements.Select(pair => (pair.Key, pair.Value)).ToList();
        public IReadOnlyList<(string fieldName, string fieldType)> NotAssignedElements => notAssignedElements.Select(pair => (pair.Key, pair.Value)).ToList();

        public GroupDefinition(string name)
        {
            Depth = 0;
            Name = name;
            elements = new Dictionary<string, string>();
            nameGenerator = new UniqueNameGenerator();
            notAssignedElements = new Dictionary<string, string>();
        }

        public void Open()
        {
            Depth++;
        }

        public void Close()
        {
            if (--Depth <= 0)
                nameGenerator.Clear();
        }

        public void Clear()
        {
            Depth = 0;
            nameGenerator.Clear();
        }

        public void ClearAndCheckAssignment()
        {
            Clear();

            foreach (var pair in elements)
                notAssignedElements.Add(pair.Key, pair.Value);
        }

        public string GetUniqueFieldName(BindingElement element)
        {
            string uniqueName = nameGenerator.GetName(element);

            if (notAssignedElements.ContainsKey(uniqueName))
                notAssignedElements.Remove(uniqueName);

            return uniqueName;
        }

        public void AddElement(BindingElement element)
        {
            string fieldName = GetUniqueFieldName(element);
            elements[fieldName] = element.GetFieldTypeName();
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

        public GroupDefinition AddGroup(string groupName)
        {
            if (definitions.TryGetValue(groupName, out var definition) == false)
                definitions[groupName] = definition = new GroupDefinition(groupName);

            return definition;
        }
    }
}