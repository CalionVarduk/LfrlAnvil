using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumnCollection : SqlColumnCollection
{
    internal SqliteColumnCollection(SqliteColumnBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlColumnCollection.Table" />
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    /// <inheritdoc cref="SqlColumnCollection.Get(string)" />
    [Pure]
    public new SqliteColumn Get(string name)
    {
        return ReinterpretCast.To<SqliteColumn>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnCollection.TryGet(string)" />
    [Pure]
    public new SqliteColumn? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteColumn>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlColumn, SqliteColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteColumn>();
    }

    /// <inheritdoc />
    protected override SqliteColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new SqliteColumn( Table, ReinterpretCast.To<SqliteColumnBuilder>( builder ) );
    }
}
