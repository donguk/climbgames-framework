
using System.IO;

namespace ClimbGames.Editor.Table
{
    public class KeyValueTableGenerator : TableCodeGenerator
    {
        private static readonly string KeyValueTableScriptGUID = "6179b5c49fb4b0c44a950139306486d5";

        protected TableSchema schema;

        public KeyValueTableGenerator(ISchema schema)
        {
            this.schema = (TableSchema)schema;
        }

        public override void Write(string path)
        {
            string recordName = WriteRecord(schema, path);

            string scriptName = schema.TableName + "Table";
            string scriptText = CreateScript(KeyValueTableScriptGUID, schema.Namespace, scriptName);

            var columns = schema.Header.Columns;
            if (columns.Count > 1)
            {
                scriptText = scriptText.Replace("#TABLE_RECORD#", recordName);

                scriptText = scriptText.Replace("#RECORD_KEYNAME#", columns[0].FieldName);
                scriptText = scriptText.Replace("#RECORD_VALUENAME#", columns[1].FieldName);

                scriptText = scriptText.Replace("#RECORD_KEYPROPERTY#", columns[0].PropertyName);
                scriptText = scriptText.Replace("#RECORD_VALUEPROPERTY#", columns[1].PropertyName);

                scriptText = scriptText.Replace("#RECORD_KEY#", columns[0].GetTypeCodeName(schema));
                scriptText = scriptText.Replace("#RECORD_VALUE#", columns[1].GetTypeCodeName(schema));

                Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            }
            else
            {
                Debug.LogError($"[KeyValueTableGenerator] table({schema.TableName}) column is invalid");
            }
        }
    }
}