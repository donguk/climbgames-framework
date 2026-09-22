
using System;
using System.IO;
using System.Text;
using log4net.Layout;

namespace ClimbGames.Editor.Table
{
    public class TableEnumGenerator : TableCodeGenerator
    {
        private static readonly string EnumScriptGUID = "5bebd521bc3a8604ebff8c5cae46af57";
        private static readonly string TableEnumScriptGUID = "bb5bd9a6232dc52488460c4b8589b1b3";

        private EnumSchema schema;
        private StringBuilder enumBuilder = new StringBuilder();

        public TableEnumGenerator(ISchema schema)
        {
            this.schema = (EnumSchema)schema;
        }

        public override void Write(string path)
        {
            string scriptName = schema.ScriptName;
            string scriptText = CreateScript(TableEnumScriptGUID, schema.Namespace, scriptName);

            StringBuilder builder = new StringBuilder();
            var definistions = schema.Definitions;
            for (int i = 0; i < definistions.Count; ++i)
            {
                var definition = definistions[i];
                if (schema.TryGetDeclaredEnum(definition.Name, out var declared) == false || declared.IsTableEnum)
                {
                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                        builder.AppendLine();
                    }

                    builder.Append(CreateEnumText(definition));
                }
                else
                {
                    if (definition.IsDeclaration && declared.IsTableEnum == false)
                        Debug.LogError($"[TableEnum] {definition.Name} is already declared in {declared.EnumType.Namespace}");
                }
            }

            scriptText = scriptText.Replace("#ENUMS#", builder.ToString());
            Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));

            builder.Clear();
        }

        string CreateEnumText(EnumDefinition definition)
        {
            string scriptText = CreateScript(EnumScriptGUID, definition.Name);

            enumBuilder.Clear();
            var values = definition.Values;
            for (int i = 0; i < values.Count; ++i)
            {
                if (EnumDefinition.NameRegex.Match(values[i]).Success)
                {
                    if (enumBuilder.Length > 0)
                    {
                        enumBuilder.AppendLine();
                        enumBuilder.Append("        ");
                    }

                    enumBuilder.Append($"{values[i]},");
                }
                else
                {
                    Debug.Log($"[EnumType] {definition.Name}: invalid value({values[i]})");
                }
            }
            scriptText = scriptText.Replace("#VALUES#", enumBuilder.ToString());

            enumBuilder.Clear();
            return scriptText;
        }
    }
}