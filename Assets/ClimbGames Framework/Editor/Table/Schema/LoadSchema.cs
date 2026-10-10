using System.Collections.Generic;

namespace ClimbGames.Editor.Table
{
    public class LoadSchema : Schema, ISchema
    {
        private List<TableSchema> tableSchemas;

        public SchemaType SchemaType => SchemaType.TableLoad;
        public override string ScriptName => "Tables";
        public IReadOnlyList<TableSchema> TableSchemas => tableSchemas;

        public LoadSchema()
        {
            tableSchemas = new List<TableSchema>();
        }

        public void AddTableSchema(TableSchema schema)
        {
            tableSchemas.Add(schema);
        }
    }
}