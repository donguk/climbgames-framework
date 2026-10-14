using System;
using System.Text;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class PrefabSchema : Schema
    {
        private BindingElement entry;
        private UniqueNameGenerator nameGenerator;

        public string PrefabName { get; private set; }
        public override string ScriptName => PrefabName;
        public GroupSchema GroupSchema { get; private set; }

        public PrefabSchema(string name)
        {
            PrefabName = name;
            nameGenerator = new UniqueNameGenerator();
            GroupSchema = new GroupSchema();
        }

        public void Read(Transform root)
        {
            nameGenerator.Clear();

            entry = new BindingElement(root.gameObject, this);
            entry.SearchElements();
        }

        public string GetUniqueFieldName(BindingElement element)
        {
            return nameGenerator.GetName(element);
        }

        public string GetFieldText()
        {
            if (entry == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            entry.RetrieveFieldText(builder);

            if (builder.Length > 0) builder.Length -= Environment.NewLine.Length;
            return builder.ToString();
        }

        public bool AssignElements()
        {
            Type type = Type.GetType($"{Namespace}.{ScriptName}, Assembly-CSharp");
            if (type != null)
            {
                var component = entry.Target.AddComponent(type);
                entry.AssignFieldValue(component);
                return true;
            }

            Debug.LogError($"[PrefabConvert] can not find type({Namespace}.{ScriptName})");
            return false;
        }
    }
}