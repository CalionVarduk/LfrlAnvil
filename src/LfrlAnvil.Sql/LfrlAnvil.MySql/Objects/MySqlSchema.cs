using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlSchema : MySqlObject, ISqlSchema
{
    internal MySqlSchema(MySqlDatabase database, MySqlSchemaBuilder builder)
        : base( builder )
    {
        Database = database;
        Objects = new MySqlObjectCollection( this, builder.Objects.Count );
    }

    public MySqlObjectCollection Objects { get; }
    public override MySqlDatabase Database { get; }
    public override string FullName => Name;

    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
