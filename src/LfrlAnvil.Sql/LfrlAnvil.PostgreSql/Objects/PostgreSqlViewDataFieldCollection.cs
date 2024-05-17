using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlViewDataFieldCollection : SqlViewDataFieldCollection
{
    internal PostgreSqlViewDataFieldCollection(SqlQueryExpressionNode source)
        : base( source ) { }

    /// <inheritdoc cref="SqlViewDataFieldCollection.View" />
    public new PostgreSqlView View => ReinterpretCast.To<PostgreSqlView>( base.View );

    /// <inheritdoc cref="SqlViewDataFieldCollection.Get(string)" />
    [Pure]
    public new PostgreSqlViewDataField Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewDataField>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlViewDataFieldCollection.TryGet(string)" />
    [Pure]
    public new PostgreSqlViewDataField? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewDataField>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlViewDataField, PostgreSqlViewDataField> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlViewDataField>();
    }

    /// <inheritdoc />
    protected override PostgreSqlViewDataField CreateDataField(string name)
    {
        return new PostgreSqlViewDataField( View, name );
    }
}
