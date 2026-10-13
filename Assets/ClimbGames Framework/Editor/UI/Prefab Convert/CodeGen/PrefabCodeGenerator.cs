using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace ClimbGames.Editor.UI
{
    public class PrefabCodeGenerator : CodeGenerator
    {
        public static readonly string TemplateGUID = "3f97c32d3d31ede44b44bb53e15dfb6d";
        public static readonly string ImplementTemplateGUID = "6308713ee9fe38a4789b4a9d023614f7";
        public static readonly string GroupTemplateGUID = "0baf94dfa46171f48978e504334e4692";

        private PrefabSchema schema;
        private StringBuilder groupBuilder = new StringBuilder();

        public PrefabCodeGenerator(PrefabSchema schema)
        {
            this.schema = schema;
        }

        public override bool Write(string path)
        {
            bool isChanged = false;

            string scriptName = schema.ScriptName;
            string scriptText = CreateScript(TemplateGUID, schema.Namespace, scriptName);

            scriptText = scriptText.Replace("#FIELDS#", schema.GetFieldText());
            scriptText = scriptText.Replace("#GROUPS#", CreateGroupText(schema.GroupSchema));

            isChanged |= Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            return isChanged;
        }

        string CreateGroupText(GroupSchema schema)
        {
            StringBuilder builder = new StringBuilder();
            var definitions = schema.Definitions;

            for (int i = 0; i < definitions.Count; ++i)
            {
                if (builder.Length > 0)
                    builder.AppendLine();

                builder.AppendLine(CreateGroupText(definitions[i]));
            }

            if (builder.Length > 0) builder.Length -= Environment.NewLine.Length;
            return builder.ToString();
        }

        string CreateGroupText(GroupDefinition definition)
        {
            string scriptText = CreateScript(GroupTemplateGUID, definition.name);

            groupBuilder.Clear();
            groupBuilder.AppendLine($"            public Transform transform;");

            var elements = definition.Elements;
            for (int i = 0; i < elements.Count; ++i)
                groupBuilder.AppendLine(elements[i].GetGroupFieldText());

            if (groupBuilder.Length > 0) groupBuilder.Length -= Environment.NewLine.Length;
            scriptText = scriptText.Replace("#FIELDS#", groupBuilder.ToString());

            groupBuilder.Clear();
            return scriptText;
        }
    }
}