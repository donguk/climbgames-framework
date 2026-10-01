
using System;
using System.Collections.Generic;
using System.IO;
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

        public static bool Write(string path, List<ISchema> schemas)
        {
            bool isChanged = false;
            foreach (var schema in schemas)
                isChanged |= Get(schema).Write(path);

            isChanged |= WriteTables(path, schemas);
            return isChanged;
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

            // 동일한 파일이라도 호출시 컴파일
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

        protected static bool WriteRecord(TableSchema schema, string path)
        {
            string scriptName = schema.TableName + "TableRecord";
            string scriptText = CreateScript(RecordScriptGUID, schema.Namespace, scriptName);

            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder propertyBuilder = new StringBuilder();
            StringBuilder writeBuilder = new StringBuilder();
            StringBuilder readBuilder = new StringBuilder();

            var columns = schema.Header.Columns;
            for (int i = 0; i < columns.Count; ++i)
            {
                var column = columns[i];

                fieldBuilder.AppendLine($"        [SerializeField] private {column.GetTypeCodeName(schema)} {column.FieldName};");
                propertyBuilder.AppendLine($"        public {column.GetTypeCodeName(schema)} {column.PropertyName} => {column.FieldName};");

                if (column.IsList)
                {
                    writeBuilder.AppendLine($"            int {column.FieldName}Count = {column.FieldName} != null ? {column.FieldName}.Count : 0;");
                    writeBuilder.AppendLine($"            bw.Write({column.FieldName}Count);");
                    writeBuilder.AppendLine($"            for (int i = 0; i < {column.FieldName}Count; ++i)");
                    writeBuilder.AppendLine($"                bw.Write({column.GetWriteCodeText()});");

                    readBuilder.AppendLine($"            {column.FieldName} = new();");
                    readBuilder.AppendLine($"            int {column.FieldName}Count = br.ReadInt32();");
                    readBuilder.AppendLine($"            for (int i = 0; i < {column.FieldName}Count; ++i)");
                    readBuilder.AppendLine($"                {column.FieldName}.Add({column.GetReadCastingCodeText(schema)}br.Read{column.GetReadTypeCodeText()}());");
                }
                else
                {
                    writeBuilder.AppendLine($"            bw.Write({column.GetWriteCodeText()});");
                    readBuilder.AppendLine($"            {column.FieldName} = {column.GetReadCastingCodeText(schema)}br.Read{column.GetReadTypeCodeText()}();");
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

        protected static bool WriteTables(string path, List<ISchema> schemas)
        {
            if (schemas.Count > 0)
            {
                string scriptText = CreateScript(TablesScriptGUID, Schema.GetNamesapce(), "Tables");

                StringBuilder propertyBuilder = new StringBuilder();
                StringBuilder caseTableBuilder = new StringBuilder();
                StringBuilder caseAssetBuilder = new StringBuilder();

                foreach (var schema in schemas)
                {
                    if (schema is TableSchema tableSchema)
                    {
                        propertyBuilder.AppendLine($"        public static {tableSchema.TableName}Table {tableSchema.TableName} {{ get; private set; }}");
                        caseTableBuilder.AppendLine($"                    case {tableSchema.TableName}Table value: {tableSchema.TableName} = value; break;");
                        caseAssetBuilder.AppendLine($"                    case \"{tableSchema.TableName}\": {tableSchema.TableName} = {tableSchema.TableName}Table.FromBytes(asset.bytes); break;");
                    }
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

                return Write(scriptText, Path.Combine(path, $"Tables.cs"));
            }

            return false;
        }
    }
}