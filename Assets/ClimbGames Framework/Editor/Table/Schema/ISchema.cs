
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

    public interface ISchema
    {
        SchemaType SchemaType { get; }
    }
}