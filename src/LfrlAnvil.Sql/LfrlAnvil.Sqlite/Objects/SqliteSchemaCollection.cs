using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteSchemaCollection : SqlSchemaCollection
{
    internal SqliteSchemaCollection(SqliteSchemaBuilderCollection source)
        : base( source ) { }

    public new SqliteSchema Default => ReinterpretCast.To<SqliteSchema>( base.Default );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    [Pure]
    public new SqliteSchema Get(string name)
    {
        return ReinterpretCast.To<SqliteSchema>( base.Get( name ) );
    }

    [Pure]
    public new SqliteSchema? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteSchema>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlSchema, SqliteSchema> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteSchema>();
    }

    protected override SqliteSchema CreateSchema(SqlSchemaBuilder builder)
    {
        return new SqliteSchema( Database, ReinterpretCast.To<SqliteSchemaBuilder>( builder ) );
    }
}
