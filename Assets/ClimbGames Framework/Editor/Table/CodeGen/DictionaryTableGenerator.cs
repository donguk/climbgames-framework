
using System.Collections.Generic;
using System.IO;

namespace ClimbGames.Editor.Table
{
    public class DictionaryTableGenerator : TableCodeGenerator
    {
        private static readonly string DictionaryTableScriptGUID = "f90f88c6da80cef48852f96d9de741eb";

        private TableSchema schema;

        public DictionaryTableGenerator(ISchema schema)
        {
            this.schema = schema as TableSchema;
        }

        public override bool Write(string path)
        {
            bool isChanged = WriteRecord(schema, path);
            string recordName = schema.ScriptName + "TableRecord";

            string scriptName = schema.ScriptName + "Table";
            string scriptText = CreateScript(DictionaryTableScriptGUID, schema.Namespace, scriptName);

            var keColumn = schema.TableHeader.KeyColumn;
            if (keColumn != null)
            {
                scriptText = scriptText.Replace("#TABLE_RECORD#", recordName);

                scriptText = scriptText.Replace("#RECORD_KEYNAME#", keColumn.FieldName);
                scriptText = scriptText.Replace("#RECORD_KEYPROPERTY#", keColumn.PropertyName);
                scriptText = scriptText.Replace("#RECORD_KEY#", keColumn.GetTypeCodeName());

                isChanged |= Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            }
            else
            {
                Debug.LogError($"[DictionaryTableGenerator] table({schema.ScriptName}) key column is null");
            }

            return isChanged;
        }
    }
}