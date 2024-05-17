using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlSchemaCollection : SqlSchemaCollection
{
    internal PostgreSqlSchemaCollection(PostgreSqlSchemaBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlSchemaCollection.Default" />
    public new PostgreSqlSchema Default => ReinterpretCast.To<PostgreSqlSchema>( base.Default );

    /// <inheritdoc cref="SqlSchemaCollection.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );

    /// <inheritdoc cref="SqlSchemaCollection.Get(string)" />
    [Pure]
    public new PostgreSqlSchema Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchema>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaCollection.TryGet(string)" />
    [Pure]
    public new PostgreSqlSchema? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchema>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlSchema, PostgreSqlSchema> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlSchema>();
    }

    /// <inheritdoc />
    protected override PostgreSqlSchema CreateSchema(SqlSchemaBuilder builder)
    {
        return new PostgreSqlSchema( Database, ReinterpretCast.To<PostgreSqlSchemaBuilder>( builder ) );
    }
}
