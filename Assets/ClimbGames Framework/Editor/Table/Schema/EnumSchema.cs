
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ClimbGames.Editor.Table
{
    public class EnumSchema : Schema
    {
        class HeaderPosition
        {
            public EnumDefinition Definition { get; set; }
            public int ColumnIndex { get; set; }
        }

        private Dictionary<string, EnumDefinition> definitions;
        private Dictionary<int, EnumDefinition> readPositions;
        private List<HeaderPosition> tableHeaderPositions;

        private Dictionary<string, DeclaredEnum> declaredEnums;
        private Dictionary<string, DeclaredEnum> assemblyEnums;

        public override SchemaType SchemaType => SchemaType.TableEnum;

        public EnumSchema()
        {
            TableName = nameof(SchemaType.TableEnum);

            definitions = new Dictionary<string, EnumDefinition>();
            readPositions = new Dictionary<int, EnumDefinition>();
            tableHeaderPositions = new List<HeaderPosition>();

            declaredEnums = new Dictionary<string, DeclaredEnum>();
            assemblyEnums = new Dictionary<string, DeclaredEnum>();
        }

        public override EnumSchema GetEnumSchema() => this;

        public bool ReadDefinition(IExcelDataReader reader)
        {
            // 이전 header 삭제
            tableHeaderPositions.Clear();

            for (int i = 0; i < reader.FieldCount; ++i)
            {
                Type fieldType = reader.GetFieldType(i);
                if (fieldType != null && fieldType == typeof(string))
                {
                    string columnName = reader.GetString(i).Trim();

                    if (EnumDefinition.TryParse(columnName, out var definition))
                    {
                        // 이미 존재하는 enum 일 경우 merge
                        if (definitions.TryGetValue(columnName, out var previous))
                            definition.Merge(previous);

                        // update definition
                        definitions[definition.Name] = definition;
                        readPositions[i] = definition;
                    }
                    else
                    {
                        if (readPositions.TryGetValue(i, out definition))
                        {
                            // end 경우 닫음
                            if (columnName != "[/enum]")
                                definition.AddValue(columnName, reader.Name.ToPascalCaseName());
                            else
                                readPositions.Remove(i);
                        }
                    }
                }
                else
                {
                    // 빈칸 인 경우 닫음
                    if (readPositions.ContainsKey(i))
                        readPositions.Remove(i);
                }
            }

            return readPositions.Count <= 0;
        }

        public void AddTableHeader(string enumName, int columnIndex)
        {
            if (definitions.TryGetValue(enumName, out var definition) == false)
                definitions[enumName] = definition = new EnumDefinition(enumName);

            tableHeaderPositions.Add(new HeaderPosition() { ColumnIndex = columnIndex, Definition = definition });
        }

        // row 에서 사용하고있는 enum value 수집
        public void ReadTableValue(IExcelDataReader reader)
        {
            for (int i = 0; i < tableHeaderPositions.Count; ++i)
            {
                var position = tableHeaderPositions[i];
                if (reader.GetFieldType(position.ColumnIndex) == typeof(string))
                {
                    string text = reader.GetString(position.ColumnIndex);
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        // list<[enum:]> 대비
                        string[] values = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var value in values)
                        {
                            string rawValue = value.Trim();
                            if (string.IsNullOrEmpty(rawValue))
                                continue;

                            position.Definition.AddValue(rawValue, reader.Name.ToPascalCaseName());
                        }
                    }
                }
            }
        }

        public string GetTypeCodeName(string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
                return "string";

            // 외부에 선언된 enum 일 경우
            if (assemblyEnums.TryGetValue(enumName, out var declaredEnum))
            {
                string fullName = declaredEnum.enumType.FullName;
                if (fullName.StartsWith($"{Namespace}."))
                    fullName = fullName.Substring(Namespace.Length + 1);

                return fullName;
            }

            return enumName;
        }

        // 스크립트에 선언 할 enum
        public IReadOnlyList<EnumDefinition> GetDeclareEnums()
        {
            var list = declaredEnums.Values.Select(x => x.definition).ToList();
            foreach (var pair in definitions)
            {
                // 이미 선언된 enum
                if (declaredEnums.ContainsKey(pair.Value.Name))
                    continue;

                // 스크립트 외부에 선언된 enum 
                if (assemblyEnums.ContainsKey(pair.Value.Name))
                    continue;

                list.Add(pair.Value);
            }

            return list;
        }

        public void ReadDeclaredEnum(string codeGenPath)
        {
            // TableEnum 이미 선언된 enum 수집
            string scriptpPath = $"{codeGenPath}/{TableName}.cs";
            if (File.Exists(scriptpPath))
            {
                string text = File.ReadAllText(scriptpPath);
                var matches = DeclaredEnum.EnumRegex.Matches(text);

                foreach (Match match in matches)
                {
                    // asmdef 가 따로 존재한느 경우 체크 필요
                    //
                    //
                    Type enumType = Type.GetType($"{Namespace}.{match.Groups[1].Value}, Assembly-CSharp");
                    declaredEnums[enumType.Name] = new DeclaredEnum(enumType);
                }
            }

            // TableEnum 외부에 선언된 enum 수집
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsEnum == false)
                        continue;

                    // table enum 인 경우 
                    if (declaredEnums.ContainsKey(type.Name))
                        continue;

                    if (string.IsNullOrEmpty(type.Namespace) || type.FullName.StartsWith($"{Namespace}."))
                        assemblyEnums[type.Name] = new DeclaredEnum(type);
                }
            }
        }
    }
}