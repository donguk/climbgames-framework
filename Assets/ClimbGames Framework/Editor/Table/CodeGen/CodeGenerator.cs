
using System.IO;
using System.Text;
using UnityEditor;

namespace ClimbGames.Editor.Table
{
    public interface ICodeGenerator
    {
        void Write(string path);
    }

    public class CodeGenerator : ICodeGenerator
    {
        public static ICodeGenerator Get(ISchema schema)
        {
            switch (schema.SchemaType)
            {
                case SchemaType.ListTable: return new ListTableGenerator(schema);
                case SchemaType.DictionaryTable: return new DictionaryTableGenerator(schema);
                case SchemaType.KeyValueTable: return new KeyValueTableGenerator(schema);
                case SchemaType.TableEnum: return new TableEnumGenerator(schema);
            }

            return new CodeGenerator();
        }

        public void Write(string path)
        {
            //throw new System.NotImplementedException();
        }
    }

    public abstract class TableCodeGenerator : ICodeGenerator
    {
        private static readonly string RecordScriptGUID = "7f64cc9c6158a0249a16ede93597f3ab";

        public abstract void Write(string path);

        protected void Write(string text, string filePath)
        {
            Directory.CreateDirectory(TableEditorSettings.CodeGenPath);

            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(filePath, text, encoding);

            AssetDatabase.ImportAsset(filePath);
        }

        protected string CreateScript(string templateGUID, string scriptName)
        {
            return CreateScript(templateGUID, null, scriptName);
        }

        protected string CreateScript(string templateGUID, string @namespace, string scriptName)
        {
            string templatePath = AssetDatabase.GUIDToAssetPath(templateGUID);
            string scriptText = File.ReadAllText(templatePath);

            scriptText = scriptText.Replace("#NAMESPACE#", @namespace);
            scriptText = scriptText.Replace("#SCRIPTNAME#", scriptName);

            return scriptText;
        }

        protected string WriteRecord(TableSchema schema, string path)
        {
            string scriptName = schema.TableName + "TableRecord";
            string scriptText = CreateScript(RecordScriptGUID, schema.Namespace, scriptName);

            StringBuilder fieldBuilder = new StringBuilder();
            var columns = schema.Header.Columns;
            for (int i = 0; i < columns.Count; ++i)
            {
                var column = columns[i];
                string intent = null;
                if (i > 0)
                    intent = "\t\t";

                fieldBuilder.Append($"{intent}[SerializeField] private {column.GetTypeCodeName(schema)} {column.FieldName};");
                if (i + 1 < columns.Count)
                    fieldBuilder.Append("\n");
            }
            scriptText = scriptText.Replace("#FIELDS#", $"{fieldBuilder.ToString()}");
            fieldBuilder.Clear();

            for (int i = 0; i < columns.Count; ++i)
            {
                var column = columns[i];
                string intent = null;
                if (i > 0)
                    intent = "\t\t";

                fieldBuilder.Append($"{intent}public {column.GetTypeCodeName(schema)} {column.PropertyName} => {column.FieldName};");
                if (i + 1 < columns.Count)
                    fieldBuilder.Append("\n");
            }
            scriptText = scriptText.Replace("#PROPERTIES#", $"{fieldBuilder.ToString()}");
            fieldBuilder.Clear();

            Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            return scriptName;
        }
    }
}