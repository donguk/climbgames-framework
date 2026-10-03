
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace ClimbGames.Editor.Table
{
    public interface ICodeGenerator
    {
        bool Write(string path);
    }

    public class TableCodeGenerator : ICodeGenerator
    {
        private static readonly string RecordScriptGUID = "7f64cc9c6158a0249a16ede93597f3ab";

        public static ICodeGenerator Get(Schema schema)
        {
            switch (schema.SchemaType)
            {
                case SchemaType.ListTable: return new ListTableGenerator(schema);
                case SchemaType.DictionaryTable: return new DictionaryTableGenerator(schema);
                case SchemaType.KeyValueTable: return new KeyValueTableGenerator(schema);
                case SchemaType.TableEnum: return new TableEnumGenerator(schema);
                case SchemaType.TableLoad: return new TableLoadGenerator(schema);
            }

            return new TableCodeGenerator(schema);
        }

        public static bool Write(string path, List<Schema> schemas)
        {
            bool isChanged = false;
            foreach (var schema in schemas)
                isChanged |= Get(schema).Write(path);

            return isChanged;
        }

        protected Schema schema;

        public TableCodeGenerator(Schema schema)
        {
            this.schema = schema;
        }

        public virtual bool Write(string path) { return false; }

        protected static bool Write(string text, string filePath)
        {
            Directory.CreateDirectory(TableEditorSettings.CodeGenPath);

            if (File.Exists(filePath))
            {
                string oldText = File.ReadAllText(filePath);
                if (oldText == text)
                    return false;
            }

            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(filePath, text, encoding);

            // 동일한 파일이라도 호출시 컴파일 발생
            AssetDatabase.ImportAsset(filePath);
            return true;
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

        protected static bool WriteRecord(Schema schema, string path)
        {
            string scriptName = schema.TableName + "TableRecord";
            string scriptText = CreateScript(RecordScriptGUID, schema.Namespace, scriptName);

            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder propertyBuilder = new StringBuilder();
            StringBuilder writeBuilder = new StringBuilder();
            StringBuilder readBuilder = new StringBuilder();

            var columns = schema.GetTableHeader().Columns;
            for (int i = 0; i < columns.Count; ++i)
            {
                var column = columns[i];

                fieldBuilder.AppendLine($"        [SerializeField] private {column.GetTypeCodeName()} {column.FieldName};");
                propertyBuilder.AppendLine($"        public {column.GetTypeCodeName()} {column.PropertyName} => {column.FieldName};");

                if (column.IsList)
                {
                    writeBuilder.AppendLine($"            int {column.FieldName}Count = {column.FieldName} != null ? {column.FieldName}.Count : 0;");
                    writeBuilder.AppendLine($"            bw.Write({column.FieldName}Count);");
                    writeBuilder.AppendLine($"            for (int i = 0; i < {column.FieldName}Count; ++i)");
                    writeBuilder.AppendLine($"                bw.Write({column.GetCastingText(true)}{column.GetWriteText()});");

                    readBuilder.AppendLine($"            {column.FieldName} = new();");
                    readBuilder.AppendLine($"            int {column.FieldName}Count = br.ReadInt32();");
                    readBuilder.AppendLine($"            for (int i = 0; i < {column.FieldName}Count; ++i)");
                    readBuilder.AppendLine($"                {column.FieldName}.Add({column.GetCastingText(false)}br.Read{column.GetReadText()}());");
                }
                else
                {
                    writeBuilder.AppendLine($"            bw.Write({column.GetCastingText(true)}{column.GetWriteText()});");
                    readBuilder.AppendLine($"            {column.FieldName} = {column.GetCastingText(false)}br.Read{column.GetReadText()}();");
                }
            }

            if (fieldBuilder.Length > 0) fieldBuilder.Length -= Environment.NewLine.Length;
            if (propertyBuilder.Length > 0) propertyBuilder.Length -= Environment.NewLine.Length;
            if (writeBuilder.Length > 0) writeBuilder.Length -= Environment.NewLine.Length;
            if (readBuilder.Length > 0) readBuilder.Length -= Environment.NewLine.Length;

            scriptText = scriptText.Replace("#FIELDS#", $"{fieldBuilder.ToString()}");
            scriptText = scriptText.Replace("#PROPERTIES#", $"{propertyBuilder.ToString()}");
            scriptText = scriptText.Replace("#WRITE_AREA#", $"{writeBuilder.ToString()}");
            scriptText = scriptText.Replace("#READ_AREA#", $"{readBuilder.ToString()}");

            fieldBuilder.Clear();
            propertyBuilder.Clear();
            writeBuilder.Clear();
            readBuilder.Clear();

            return Write(scriptText, Path.Combine(path, $"{scriptName}.cs"));
        }
    }
}