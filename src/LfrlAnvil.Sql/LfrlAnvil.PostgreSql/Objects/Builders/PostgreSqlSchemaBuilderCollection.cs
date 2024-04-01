using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal PostgreSqlSchemaBuilderCollection() { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlSchemaBuilder Default => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Default );

    [Pure]
    public new PostgreSqlSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Get( name ) );
    }

    [Pure]
    public new PostgreSqlSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.TryGet( name ) );
    }

    public new PostgreSqlSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Create( name ) );
    }

    public new PostgreSqlSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, PostgreSqlSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlSchemaBuilder>();
    }

    protected override PostgreSqlSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new PostgreSqlSchemaBuilder( Database, name );
    }
}
