using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlSchema : SqlObject, ISqlSchema
{
    protected SqlSchema(SqlDatabase database, SqlSchemaBuilder builder, SqlObjectCollection objects)
        : base( database, builder )
    {
        Objects = objects;
        Objects.SetSchema( this );
    }

    public SqlObjectCollection Objects { get; }
    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
