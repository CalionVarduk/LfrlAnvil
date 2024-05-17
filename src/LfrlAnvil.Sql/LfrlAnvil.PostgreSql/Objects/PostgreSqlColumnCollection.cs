using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlColumnCollection : SqlColumnCollection
{
    internal PostgreSqlColumnCollection(PostgreSqlColumnBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlColumnCollection.Table" />
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    /// <inheritdoc cref="SqlColumnCollection.Get(string)" />
    [Pure]
    public new PostgreSqlColumn Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumn>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnCollection.TryGet(string)" />
    [Pure]
    public new PostgreSqlColumn? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumn>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlColumn, PostgreSqlColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlColumn>();
    }

    /// <inheritdoc />
    protected override PostgreSqlColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new PostgreSqlColumn( Table, ReinterpretCast.To<PostgreSqlColumnBuilder>( builder ) );
    }
}
