
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public class DeclaredEnum
    {
        public Type EnumType { get; private set; }
        public bool IsTableEnum { get; private set; }

        public DeclaredEnum(Type enumType, bool isTableEnum)
        {
            EnumType = enumType;
            IsTableEnum = isTableEnum;
        }
    }

    public class EnumSchema : Schema, ISchema
    {
        private static readonly Regex DeclaredRegex = new Regex(@"\benum\s+([A-Za-z_][A-Za-z0-9_]*)\b");

        class HeaderPosition
        {
            public EnumDefinition Definition { get; set; }
            public int ColumnIndex { get; set; }
        }

        private Dictionary<string, EnumDefinition> definitions;
        private Dictionary<int, EnumDefinition> readPositions;
        private Dictionary<string, DeclaredEnum> declaredEnums;
        private List<HeaderPosition> headerPositions;

        public string ScriptName => nameof(SchemaType.TableEnum);
        public SchemaType SchemaType => SchemaType.TableEnum;
        public IReadOnlyList<EnumDefinition> Definitions => definitions.Values.ToList();
        public int HeaderCount => headerPositions.Count;

        public EnumSchema()
        {
            definitions = new Dictionary<string, EnumDefinition>();
            readPositions = new Dictionary<int, EnumDefinition>();
            headerPositions = new List<HeaderPosition>();
        }

        public bool Read(IExcelDataReader reader)
        {
            headerPositions.Clear();

            for (int i = 0; i < reader.FieldCount; ++i)
            {
                Type fieldType = reader.GetFieldType(i);
                if (fieldType != null && fieldType == typeof(string))
                {
                    string columnName = reader.GetString(i).Trim();

                    if (EnumDefinition.TryParse(columnName, out var definition))
                    {
                        if (definitions.TryGetValue(columnName, out var previous))
                            definition.Merge(previous);

                        // update definition
                        definitions[definition.Name] = definition;
                        readPositions[i] = definition;
                    }
                    else
                    {
                        if (readPositions.TryGetValue(i, out definition))
                            definition.AddValue(columnName);
                    }
                }
                else if (readPositions.ContainsKey(i))
                {
                    // close 
                    readPositions.Remove(i);
                }
            }

            return readPositions.Count <= 0;
        }

        public void AddHeader(string enumName, int columnIndex)
        {
            if (definitions.TryGetValue(enumName, out var definition) == false)
                definitions[enumName] = definition = new EnumDefinition(enumName);

            //
            headerPositions.Add(new HeaderPosition() { ColumnIndex = columnIndex, Definition = definition });
        }

        public void ReadHeaderValues(IExcelDataReader reader)
        {
            for (int i = 0; i < headerPositions.Count; ++i)
            {
                var position = headerPositions[i];
                string value = reader.GetValue(position.ColumnIndex).ToString();
                position.Definition.AddValue(value);
            }
        }

        public string GetCodeName(string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
                return "string";

            if (TryGetDeclaredEnum(enumName, out var declared) && declared.IsTableEnum == false)
            {
                string fullName = declared.EnumType.FullName;
                if (fullName.StartsWith($"{Namespace}."))
                    fullName = fullName.Substring(Namespace.Length + 1);

                return fullName;
            }

            return enumName;
        }

        public bool TryGetDeclaredEnum(string enumName, out DeclaredEnum declared)
        {
            if (declaredEnums == null)
            {
                // table enums 필터
                HashSet<string> tableEnums = new HashSet<string>();
                string tableEnumFilePath = Path.Combine(TableEditorSettings.CodeGenPath, $"{ScriptName}.cs");
                if (File.Exists(tableEnumFilePath))
                {
                    string text = File.ReadAllText(tableEnumFilePath);
                    foreach (Match match in DeclaredRegex.Matches(text))
                        tableEnums.Add(match.Groups[1].Value);
                }

                declaredEnums = new Dictionary<string, DeclaredEnum>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsEnum == false)
                            continue;

                        if (string.IsNullOrEmpty(type.Namespace) || type.Namespace.StartsWith(Namespace))
                            declaredEnums[type.Name] = new DeclaredEnum(type, tableEnums.Contains(type.Name));
                    }
                }
            }

            return declaredEnums.TryGetValue(enumName, out declared);
        }
    }
}