using System;
using System.IO;
using System.Text;

namespace ClimbGames.Editor.Table
{
    public class TableLoadGenerator : TableCodeGenerator
    {
        private static readonly string TablesScriptGUID = "24e9f7f1262fe3d49a67b68f69950d9e";

        private LoadSchema schema;

        public TableLoadGenerator(ISchema schema)
        {
            this.schema = schema as LoadSchema;
        }

        public override bool Write(string path)
        {
            var tableSchemas = schema.TableSchemas;

            string scriptName = schema.ScriptName;
            string scriptText = CreateScript(TablesScriptGUID, schema.Namespace, "Tables");

            StringBuilder propertyBuilder = new StringBuilder();
            StringBuilder caseTableBuilder = new StringBuilder();
            StringBuilder caseAssetBuilder = new StringBuilder();

            foreach (var tableSchema in tableSchemas)
            {
                if (tableSchema.SchemaType == SchemaType.TableEnum)
                    continue;

                propertyBuilder.AppendLine($"        public static {tableSchema.ScriptName}Table {tableSchema.ScriptName} {{ get; private set; }}");
                caseTableBuilder.AppendLine($"                    case {tableSchema.ScriptName}Table value: {tableSchema.ScriptName} = value; break;");
                caseAssetBuilder.AppendLine($"                    case \"{tableSchema.ScriptName}\": {tableSchema.ScriptName} = Tables<{tableSchema.ScriptName}Table>.FromBytes(asset.bytes); break;");
            }

            if (propertyBuilder.Length > 0) propertyBuilder.Length -= Environment.NewLine.Length;
            if (caseTableBuilder.Length > 0) caseTableBuilder.Length -= Environment.NewLine.Length;
            if (caseAssetBuilder.Length > 0) caseAssetBuilder.Length -= Environment.NewLine.Length;

            scriptText = scriptText.Replace("#PROPERTIES#", $"{propertyBuilder.ToString()}");
            scriptText = scriptText.Replace("#CASE_TABLES#", $"{caseTableBuilder.ToString()}");
            scriptText = scriptText.Replace("#CASE_ASSETS#", $"{caseAssetBuilder.ToString()}");

            propertyBuilder.Clear();
            caseTableBuilder.Clear();
            caseAssetBuilder.Clear();

            return Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
        }
    }
}