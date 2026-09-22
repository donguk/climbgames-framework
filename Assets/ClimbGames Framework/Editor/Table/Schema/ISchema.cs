
namespace ClimbGames.Editor.Table
{
    public enum SchemaType
    {
        None,
        ListTable,
        DictionaryTable,
        KeyValueTable,
        TableEnum,
    }

    public interface ISchema
    {
        SchemaType SchemaType { get; }
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

        public Schema()
        {
            Namespace = GetNamesapce();
        }
    }
}