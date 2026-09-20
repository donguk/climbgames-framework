using System;
using System.Collections.Generic;

namespace ClimbGames.Editor.Table
{
    public class ColumnInfo
    {
        public int Index { get; private set; }
        public string FieldName { get; private set; }
        public string PropertyName { get; private set; }
        public Type FieldType { get; private set; }

        public ColumnInfo(int index, string name)
        {
            Index = index;
            FieldName = name.ToCamelCaseName();
            PropertyName = name.ToPascalCaseName();
        }

        public ColumnInfo(int index, string name, Type type) : this(index, name)
        {
            FieldType = type;
        }

        public void SetType(Type type)
        {
            FieldType = type;
        }
    }

    public static class FieldType__
    {

    }
}