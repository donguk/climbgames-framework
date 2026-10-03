
using System.Collections.Generic;

namespace ClimbGames.Editor.Table
{
    public enum SchemaType
    {
        None,
        ListTable,
        DictionaryTable,
        KeyValueTable,
        TableEnum,
        TableLoad,
    }

    public abstract class Schema
    {
        public static string GetNamesapce()
        {
            string @amespace = FrameworkEditorSettings.instance.ProjectNamesapce;
            if (string.IsNullOrEmpty(@amespace))
                @amespace = "ClimbGames";

            return @amespace;
        }

        public string Namespace { get; private set; }

        public string TableName { get; protected set; }
        public abstract SchemaType SchemaType { get; }

        public Schema()
        {
            Namespace = GetNamesapce();
        }

        public virtual TableHeader GetTableHeader() => null;
        public virtual EnumSchema GetEnumSchema() => null;
    }
}