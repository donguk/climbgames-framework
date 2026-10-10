
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClimbGames.Editor.Table
{
    public class TableCodeGenerator : CodeGenerator
    {
        private static readonly string RecordScriptGUID = "7f64cc9c6158a0249a16ede93597f3ab";

        public static ICodeGenerator Get(ISchema schema)
        {
            switch (schema.SchemaType)
            {
                case SchemaType.ListTable: return new ListTableGenerator(schema);
                case SchemaType.DictionaryTable: return new DictionaryTableGenerator(schema);
                case SchemaType.KeyValueTable: return new KeyValueTableGenerator(schema);
                case SchemaType.TableEnum: return new TableEnumGenerator(schema);
                case SchemaType.TableLoad: return new TableLoadGenerator(schema);
            }

            return new TableCodeGenerator();
        }

        public static bool Write(string path, List<ISchema> schemas)
        {
            bool isChanged = false;
            foreach (var schema in schemas)
                isChanged |= Get(schema).Write(path);

            return isChanged;
        }

        protected static bool WriteRecord(TableSchema schema, string path)
        {
            string scriptName = schema.ScriptName + "TableRecord";
            string scriptText = CreateScript(RecordScriptGUID, schema.Namespace, scriptName);

            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder propertyBuilder = new StringBuilder();
            StringBuilder writeBuilder = new StringBuilder();
            StringBuilder readBuilder = new StringBuilder();

            var columns = schema.TableHeader.Columns;
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