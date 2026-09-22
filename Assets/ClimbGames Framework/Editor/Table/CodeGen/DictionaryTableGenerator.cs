
using System.Collections.Generic;
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

        public override bool Write(string path)
        {
            bool isChanged = WriteRecord(schema, path);
            string recordName = schema.TableName + "TableRecord";

            string scriptName = schema.TableName + "Table";
            string scriptText = CreateScript(DictionaryTableScriptGUID, schema.Namespace, scriptName);

            var keColumn = schema.Header.KeyColumn;
            if (keColumn != null)
            {
                scriptText = scriptText.Replace("#TABLE_RECORD#", recordName);

                scriptText = scriptText.Replace("#RECORD_KEYNAME#", keColumn.FieldName);
                scriptText = scriptText.Replace("#RECORD_KEYPROPERTY#", keColumn.PropertyName);
                scriptText = scriptText.Replace("#RECORD_KEY#", keColumn.GetTypeCodeName(schema));

                isChanged |= Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            }
            else
            {
                Debug.LogError($"[DictionaryTableGenerator] table({schema.TableName}) key column is null");
            }

            return isChanged;
        }
    }
}