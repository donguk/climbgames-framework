
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
        public string Namespace { get; private set; }

        public Schema()
        {
            Namespace = FrameworkEditorSettings.instance.ProjectNamesapce;
            if (string.IsNullOrEmpty(Namespace))
                Namespace = "ClimbGames";
        }
    }
}