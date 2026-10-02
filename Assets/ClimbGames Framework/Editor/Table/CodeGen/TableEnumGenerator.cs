
using System;
using System.IO;
using System.Text;

namespace ClimbGames.Editor.Table
{
    public class TableEnumGenerator : TableCodeGenerator
    {
        private static readonly string EnumScriptGUID = "5bebd521bc3a8604ebff8c5cae46af57";
        private static readonly string TableEnumScriptGUID = "bb5bd9a6232dc52488460c4b8589b1b3";

        private StringBuilder enumBuilder = new StringBuilder();

        public TableEnumGenerator(Schema schema) : base(schema)
        {
        }

        public override bool Write(string path)
        {
            string scriptName = schema.TableName;
            string scriptText = CreateScript(TableEnumScriptGUID, schema.Namespace, scriptName);

            StringBuilder builder = new StringBuilder();
            var enumSchema = schema.GetEnumSchema();

            var definistions = enumSchema.GetDeclareEnums();
            for (int i = 0; i < definistions.Count; ++i)
            {
                var definition = definistions[i];
                if (builder.Length > 0)
                    builder.AppendLine();

                builder.AppendLine(CreateEnumText(definition));
            }

            if (builder.Length > 0) builder.Length -= Environment.NewLine.Length;

            scriptText = scriptText.Replace("#ENUMS#", builder.ToString());
            builder.Clear();

            return Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
        }

        string CreateEnumText(EnumDefinition definition)
        {
            string scriptText = CreateScript(EnumScriptGUID, definition.Name);
            enumBuilder.Clear();

            var values = definition.Values;
            for (int i = 0; i < values.Count; ++i)
            {
                if (EnumDefinition.NameRegex.Match(values[i]).Success == false)
                {
                    Debug.Log($"[EnumType] {definition.Name}: invalid value({values[i]})");
                    continue;
                }
                enumBuilder.AppendLine($"        {values[i]},");
            }

            if (enumBuilder.Length > 0) enumBuilder.Length -= Environment.NewLine.Length;

            scriptText = scriptText.Replace("#VALUES#", enumBuilder.ToString());
            enumBuilder.Clear();

            return scriptText;
        }
    }
}