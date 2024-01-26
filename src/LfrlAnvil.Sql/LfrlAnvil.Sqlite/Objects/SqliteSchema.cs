using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteSchema : SqliteObject, ISqlSchema
{
    internal SqliteSchema(SqliteDatabase database, SqliteSchemaBuilder builder)
        : base( builder )
    {
        Database = database;
        Objects = new SqliteObjectCollection( this, builder.Objects.Count );
    }

    public SqliteObjectCollection Objects { get; }
    public override SqliteDatabase Database { get; }
    public override string FullName => Name;

    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
