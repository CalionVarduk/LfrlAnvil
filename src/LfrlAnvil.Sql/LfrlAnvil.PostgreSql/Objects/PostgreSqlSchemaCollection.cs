using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlSchemaCollection : SqlSchemaCollection
{
    internal PostgreSqlSchemaCollection(PostgreSqlSchemaBuilderCollection source)
        : base( source ) { }

    public new PostgreSqlSchema Default => ReinterpretCast.To<PostgreSqlSchema>( base.Default );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );

    [Pure]
    public new PostgreSqlSchema Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchema>( base.Get( name ) );
    }

    [Pure]
    public new PostgreSqlSchema? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchema>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlSchema, PostgreSqlSchema> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlSchema>();
    }

    protected override PostgreSqlSchema CreateSchema(SqlSchemaBuilder builder)
    {
        return new PostgreSqlSchema( Database, ReinterpretCast.To<PostgreSqlSchemaBuilder>( builder ) );
    }
}
