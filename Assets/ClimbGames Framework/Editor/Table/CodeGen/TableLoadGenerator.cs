using System;
using System.IO;
using System.Text;

namespace ClimbGames.Editor.Table
{
    public class TableLoadGenerator : TableCodeGenerator
    {
        private static readonly string TablesScriptGUID = "24e9f7f1262fe3d49a67b68f69950d9e";

        public TableLoadGenerator(Schema schema) : base(schema)
        {
        }

        public override bool Write(string path)
        {
            var loadSchema = schema as LoadSchema;
            var schemas = loadSchema.TableSchemas;

            string scriptName = schema.TableName;
            string scriptText = CreateScript(TablesScriptGUID, Schema.GetNamesapce(), "Tables");

            StringBuilder propertyBuilder = new StringBuilder();
            StringBuilder caseTableBuilder = new StringBuilder();
            StringBuilder caseAssetBuilder = new StringBuilder();

            foreach (var schema in schemas)
            {
                if (schema.SchemaType == SchemaType.TableEnum)
                    continue;

                propertyBuilder.AppendLine($"        public static {schema.TableName}Table {schema.TableName} {{ get; private set; }}");
                caseTableBuilder.AppendLine($"                    case {schema.TableName}Table value: {schema.TableName} = value; break;");
                caseAssetBuilder.AppendLine($"                    case \"{schema.TableName}\": {schema.TableName} = Tables<{schema.TableName}Table>.FromBytes(asset.bytes); break;");
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