
using System.IO;

namespace ClimbGames.Editor.Table
{
    public class KeyValueTableGenerator : TableCodeGenerator
    {
        private static readonly string KeyValueTableScriptGUID = "6179b5c49fb4b0c44a950139306486d5";

        protected TableSchema schema;

        public KeyValueTableGenerator(TableSchema schema)
        {
            this.schema = schema;
        }

        public override void Write(string path)
        {
            string scriptName = schema.TableName + "Table";
            string scriptText = CreateScript(KeyValueTableScriptGUID, schema.Namespace, scriptName);

            var columnList = schema.ColumnList;
            if (columnList.Count > 1)
            {
                scriptText = scriptText.Replace("#RECORDKEYNAME#", columnList[0].FieldName);
                scriptText = scriptText.Replace("#RECORDVALUENAME#", columnList[1].FieldName);

                scriptText = scriptText.Replace("#RECORDKEY#", columnList[0].FieldType.ToCodeName());
                scriptText = scriptText.Replace("#RECORDVALUE#", columnList[1].FieldType.ToCodeName());

                Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            }
            else
            {
                Debug.LogError($"[KeyValueTableGenerator] table({schema.TableName}) column is invalid");
            }
        }
    }
}