using System;
using System.IO;
using System.Text;
using ClimbGames.UI;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class PrefabCodeGenerator : CodeGenerator
    {
        public static readonly string TemplateGUID = "3f97c32d3d31ede44b44bb53e15dfb6d";
        public static readonly string ImplementTemplateGUID = "6308713ee9fe38a4789b4a9d023614f7";
        public static readonly string GroupTemplateGUID = "0baf94dfa46171f48978e504334e4692";

        private PrefabSchema schema;
        private Type baseType;
        private StringBuilder groupBuilder = new StringBuilder();

        public PrefabCodeGenerator(PrefabSchema schema, Type baseType)
        {
            this.schema = schema;
            this.baseType = baseType;
        }

        string GetTypeName(Type type)
        {
            string fullName = type.FullName;
            return fullName.TrimStart("ClimbGames.UI.", "UnityEngine.UI.", "UnityEngine.", "TMPro.", $"{schema.Namespace}.");
        }

        public override bool Write(string path)
        {
            bool isChanged = false;

            string scriptName = schema.ScriptName;
            string scriptText = CreateScript(TemplateGUID, schema.Namespace, scriptName);
            Type baseType = this.baseType ?? typeof(UIBase);
            string fullName = GetTypeName(baseType);

            scriptText = scriptText.Replace("#BASETYPE#", fullName);
            scriptText = scriptText.Replace("#FIELDS#", schema.GetFieldText());
            scriptText = scriptText.Replace("#GROUPS#", CreateGroupText(schema.GroupSchema));

            isChanged |= Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            isChanged |= WriteImpl(path);

            return isChanged;
        }

        bool WriteImpl(string path)
        {
            bool isChanged = false;

            string scriptName = schema.ScriptName;
            string filePath = Path.Combine(path, $"{scriptName}.Impl.cs");
            if (File.Exists(filePath) == false)
            {
                string scriptText = CreateScript(ImplementTemplateGUID, schema.Namespace, scriptName);

                Type baseType = this.baseType ?? typeof(UIBase);
                string fullName = GetTypeName(baseType);

                StringBuilder builder = new StringBuilder();
                if (baseType == typeof(UIBase))
                {
                    builder.AppendLine("        protected override async UniTask OnInitialize()");
                    builder.AppendLine("        {");
                    builder.AppendLine();
                    builder.AppendLine();
                    builder.AppendLine("            await base.OnInitialize();");
                    builder.AppendLine("        }");

                }
                else
                {
                    builder.AppendLine("        void OnEnable()");
                    builder.AppendLine("        {");
                    builder.AppendLine();
                    builder.AppendLine("        }");
                }

                if (builder.Length > 0) builder.Length -= Environment.NewLine.Length;

                scriptText = scriptText.Replace("#BASETYPE#", fullName);
                scriptText = scriptText.Replace("#INITIALIZE#", builder.ToString());

                isChanged |= Write(scriptText, filePath);
            }

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
            string scriptText = CreateScript(GroupTemplateGUID, definition.Name);

            groupBuilder.Clear();
            groupBuilder.AppendLine($"            public Transform transform;");

            var elements = definition.Elements;
            for (int i = 0; i < elements.Count; ++i)
                groupBuilder.AppendLine($"            public {elements[i].fieldType} {elements[i].fieldName};");

            if (groupBuilder.Length > 0) groupBuilder.Length -= Environment.NewLine.Length;
            scriptText = scriptText.Replace("#FIELDS#", groupBuilder.ToString());

            groupBuilder.Clear();
            return scriptText;
        }
    }
}