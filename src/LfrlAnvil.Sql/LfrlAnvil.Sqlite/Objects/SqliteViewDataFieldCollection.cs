using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteViewDataFieldCollection : SqlViewDataFieldCollection
{
    internal SqliteViewDataFieldCollection(SqlQueryExpressionNode source)
        : base( source ) { }

    /// <inheritdoc cref="SqlViewDataFieldCollection.View" />
    public new SqliteView View => ReinterpretCast.To<SqliteView>( base.View );

    /// <inheritdoc cref="SqlViewDataFieldCollection.Get(string)" />
    [Pure]
    public new SqliteViewDataField Get(string name)
    {
        return ReinterpretCast.To<SqliteViewDataField>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlViewDataFieldCollection.TryGet(string)" />
    [Pure]
    public new SqliteViewDataField? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteViewDataField>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlViewDataField, SqliteViewDataField> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteViewDataField>();
    }

    /// <inheritdoc />
    protected override SqliteViewDataField CreateDataField(string name)
    {
        return new SqliteViewDataField( View, name );
    }
}
