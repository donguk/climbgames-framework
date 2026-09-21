
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

        private Dictionary<string, EnumDefinition> definitions;
        private Dictionary<int, EnumDefinition> readEnums;
        private Dictionary<string, DeclaredEnum> declaredEnums;

        public string ScriptName => nameof(SchemaType.TableEnum);
        public SchemaType SchemaType => SchemaType.TableEnum;
        public IReadOnlyList<EnumDefinition> Definitions => definitions.Values.ToList();

        public EnumSchema()
        {
            definitions = new Dictionary<string, EnumDefinition>();
            readEnums = new Dictionary<int, EnumDefinition>();
        }

        public void AddDefinition(string enumName)
        {
            if (definitions.ContainsKey(enumName) == false)
                definitions.Add(enumName, new EnumDefinition(enumName));
        }

        public bool Read(IExcelDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                Type fieldType = reader.GetFieldType(i);
                if (fieldType != null && fieldType == typeof(string))
                {
                    string columnName = reader.GetString(i).Trim();

                    if (EnumDefinition.TryParse(columnName, out var declaration))
                    {
                        if (definitions.ContainsKey(declaration.Name) == false)
                        {
                            declaration.Index = i;
                            definitions.Add(declaration.Name, declaration);

                            readEnums[i] = declaration;
                        }
                    }
                    else
                    {
                        if (readEnums.TryGetValue(i, out declaration))
                            declaration.AddValue(columnName);
                    }
                }
                else if (readEnums.ContainsKey(i))
                {
                    // close 
                    readEnums.Remove(i);
                }
            }

            return readEnums.Count <= 0;
        }

        public string GetCodeName(string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
                return "string";

            if (TryGetDeclaredEnum(enumName, out var declared))
            {
                string @namespace = declared.EnumType.Namespace;
                if (@namespace.StartsWith(Namespace))
                    @namespace = @namespace.Substring(Namespace.Length + 1);

                if (string.IsNullOrEmpty(@namespace) == false)
                    return $"{@namespace}.{enumName}";
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