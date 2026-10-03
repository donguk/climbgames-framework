using System.Collections.Generic;

namespace ClimbGames.Editor.Table
{
    public class LoadSchema : Schema
    {
        private List<TableSchema> tableSchemas;

        public override SchemaType SchemaType => SchemaType.TableLoad;
        public IReadOnlyList<TableSchema> TableSchemas => tableSchemas;

        public LoadSchema()
        {
            TableName = "Tables";
            tableSchemas = new List<TableSchema>();
        }

        public void AddTableSchema(TableSchema schema)
        {
            tableSchemas.Add(schema);
        }
    }
}