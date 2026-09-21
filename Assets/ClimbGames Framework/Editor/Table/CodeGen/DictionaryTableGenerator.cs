
using System.IO;

namespace ClimbGames.Editor.Table
{
    public class DictionaryTableGenerator : TableCodeGenerator
    {
        private static readonly string DictionaryTableScriptGUID = "f90f88c6da80cef48852f96d9de741eb";

        protected TableSchema schema;

        public DictionaryTableGenerator(ISchema schema)
        {
            this.schema = (TableSchema)schema;
        }

        public override void Write(string path)
        {
            string recordName = WriteRecord(schema, path);

            string scriptName = schema.TableName + "Table";
            string scriptText = CreateScript(DictionaryTableScriptGUID, schema.Namespace, scriptName);

            var keColumn = schema.Header.KeyColumn;
            if (keColumn != null)
            {
                scriptText = scriptText.Replace("#RECORDKEYNAME#", keColumn.FieldName);
                scriptText = scriptText.Replace("#RECORDKEY#", keColumn.GetTypeCodeName(schema));
                scriptText = scriptText.Replace("#TABLERECORD#", recordName);

                Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            }
            else
            {
                Debug.LogError($"[DictionaryTableGenerator] table({schema.TableName}) key column is null");
            }
        }
    }
}