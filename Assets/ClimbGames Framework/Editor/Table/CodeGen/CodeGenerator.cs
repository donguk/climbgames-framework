
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace ClimbGames.Editor.Table
{
    public interface ICodeGenerator
    {
        void Write(string path);
    }

    public class TableCodeGenerator : ICodeGenerator
    {
        private static readonly string RecordScriptGUID = "7f64cc9c6158a0249a16ede93597f3ab";
        private static readonly string TablesScriptGUID = "24e9f7f1262fe3d49a67b68f69950d9e";

        public static ICodeGenerator Get(ISchema schema)
        {
            switch (schema.SchemaType)
            {
                case SchemaType.ListTable: return new ListTableGenerator(schema);
                case SchemaType.DictionaryTable: return new DictionaryTableGenerator(schema);
                case SchemaType.KeyValueTable: return new KeyValueTableGenerator(schema);
                case SchemaType.TableEnum: return new TableEnumGenerator(schema);
            }

            return new TableCodeGenerator();
        }

        public static void Write(string path, List<ISchema> schemas)
        {
            foreach (var schema in schemas)
                Get(schema).Write(path);

            WriteTables(path, schemas);
        }

        public virtual void Write(string path) { }

        protected static void Write(string text, string filePath)
        {
            Directory.CreateDirectory(TableEditorSettings.CodeGenPath);

            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(filePath, text, encoding);

            AssetDatabase.ImportAsset(filePath);
        }

        protected static string CreateScript(string templateGUID, string scriptName)
        {
            return CreateScript(templateGUID, null, scriptName);
        }

        protected static string CreateScript(string templateGUID, string @namespace, string scriptName)
        {
            string templatePath = AssetDatabase.GUIDToAssetPath(templateGUID);
            string scriptText = File.ReadAllText(templatePath);

            scriptText = scriptText.Replace("#NAMESPACE#", @namespace);
            scriptText = scriptText.Replace("#SCRIPTNAME#", scriptName);

            return scriptText;
        }

        protected static string WriteRecord(TableSchema schema, string path)
        {
            string scriptName = schema.TableName + "TableRecord";
            string scriptText = CreateScript(RecordScriptGUID, schema.Namespace, scriptName);

            StringBuilder builder = new StringBuilder();
            var columns = schema.Header.Columns;
            for (int i = 0; i < columns.Count; ++i)
            {
                var column = columns[i];
                string intent = null;
                if (builder.Length > 0)
                    intent = "\t\t";

                builder.Append($"{intent}[SerializeField] private {column.GetTypeCodeName(schema)} {column.FieldName};");
                if (i + 1 < columns.Count)
                    builder.Append("\n");
            }
            scriptText = scriptText.Replace("#FIELDS#", $"{builder.ToString()}");
            builder.Clear();

            for (int i = 0; i < columns.Count; ++i)
            {
                var column = columns[i];
                string intent = null;
                if (builder.Length > 0)
                    intent = "\t\t";

                builder.Append($"{intent}public {column.GetTypeCodeName(schema)} {column.PropertyName} => {column.FieldName};");
                if (i + 1 < columns.Count)
                    builder.Append("\n");
            }
            scriptText = scriptText.Replace("#PROPERTIES#", $"{builder.ToString()}");
            builder.Clear();

            Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
            return scriptName;
        }

        protected static void WriteTables(string path, List<ISchema> schemas)
        {
            if (schemas.Count > 0)
            {
                string scriptText = CreateScript(TablesScriptGUID, Schema.GetNamesapce());

                StringBuilder builder = new StringBuilder();
                foreach (var schema in schemas)
                {
                    if (schema is TableSchema tableSchema)
                    {
                        if (builder.Length > 0)
                        {
                            builder.AppendLine();
                            builder.Append("\t\t");
                        }

                        builder.Append($"public static {tableSchema.TableName}Table {tableSchema.TableName} {{ get; private set; }}");
                    }
                }

                scriptText = scriptText.Replace("#PROPERTIES#", $"{builder.ToString()}");
                builder.Clear();

                Write(scriptText, Path.Combine(path, $"Tables.cs"));
            }
        }
    }
}