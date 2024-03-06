using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal SqliteSchemaBuilderCollection() { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteSchemaBuilder Default => ReinterpretCast.To<SqliteSchemaBuilder>( base.Default );

    [Pure]
    public new SqliteSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.Get( name ) );
    }

    [Pure]
    public new SqliteSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.TryGet( name ) );
    }

    public new SqliteSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.Create( name ) );
    }

    public new SqliteSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, SqliteSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteSchemaBuilder>();
    }

    protected override SqliteSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new SqliteSchemaBuilder( Database, name );
    }
}
