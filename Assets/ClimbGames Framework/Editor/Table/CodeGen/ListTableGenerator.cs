
using System.IO;

namespace ClimbGames.Editor.Table
{
    public class ListTableGenerator : TableCodeGenerator
    {
        private static readonly string ListTableScriptGUID = "81581d2875974034d809155ef69fc97c";

        protected TableSchema schema;

        public ListTableGenerator(ISchema schema)
        {
            this.schema = (TableSchema)schema;
        }

        public override bool Write(string path)
        {
            bool isChanged = WriteRecord(schema, path);
            string recordName = schema.TableName + "TableRecord";

            string scriptName = schema.TableName + "Table";
            string scriptText = CreateScript(ListTableScriptGUID, schema.Namespace, scriptName);

            scriptText = scriptText.Replace("#TABLERECORD#", recordName);

            isChanged |= Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));

            return isChanged;
        }
    }
}