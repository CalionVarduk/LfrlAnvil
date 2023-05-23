using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteSchema : SqliteObject, ISqlSchema
{
    private SqliteSchema(SqliteDatabase database, SqliteSchemaBuilder builder)
        : base( builder )
    {
        Database = database;
        Objects = new SqliteObjectCollection( this, builder.Objects.Count );
    }

    public SqliteObjectCollection Objects { get; }
    public override SqliteDatabase Database { get; }
    public override string FullName => Name;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static SqliteSchema Create(
        SqliteDatabase database,
        SqliteSchemaBuilder builder,
        RentedMemorySequence<SqliteObjectBuilder> tables)
    {
        var result = new SqliteSchema( database, builder );
        result.Objects.Populate( builder.Objects, tables );
        return result;
    }

    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
