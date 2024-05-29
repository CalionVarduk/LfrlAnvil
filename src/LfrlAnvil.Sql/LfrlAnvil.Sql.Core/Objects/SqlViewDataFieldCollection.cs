using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlViewDataFieldCollection" />
public abstract class SqlViewDataFieldCollection : ISqlViewDataFieldCollection
{
    private readonly Dictionary<string, SqlViewDataField> _map;
    private SqlView? _view;

    /// <summary>
    /// Creates a new <see cref="SqlViewDataFieldCollection"/> instance.
    /// </summary>
    /// <param name="source">Source query expression.</param>
    protected SqlViewDataFieldCollection(SqlQueryExpressionNode source)
    {
        _map = new Dictionary<string, SqlViewDataField>( capacity: source.Selection.Count, comparer: SqlHelpers.NameComparer );
        _view = null;
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <inheritdoc cref="ISqlViewDataFieldCollection.View" />
    public SqlView View
    {
        get
        {
            Assume.IsNotNull( _view );
            return _view;
        }
    }

    ISqlView ISqlViewDataFieldCollection.View => View;

    /// <inheritdoc />
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <inheritdoc cref="ISqlViewDataFieldCollection.Get(string)" />
    [Pure]
    public SqlViewDataField Get(string name)
    {
        return _map[name];
    }

    /// <inheritdoc cref="ISqlViewDataFieldCollection.TryGet(string)" />
    [Pure]
    public SqlViewDataField? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{T}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{T}"/> instance.</returns>
    [Pure]
    public SqlObjectEnumerator<SqlViewDataField> GetEnumerator()
    {
        return new SqlObjectEnumerator<SqlViewDataField>( _map );
    }

    internal void SetView(SqlView view, SqlQueryExpressionNode source)
    {
        Assume.IsNull( _view );
        Assume.Equals( view.DataFields, this );
        _view = view;

        source.ReduceKnownDataFieldExpressions( e => _map.Add( e.Key, CreateDataField( e.Key ) ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlViewDataField"/> instance.
    /// </summary>
    /// <param name="name">Data field's name.</param>
    /// <returns>New <see cref="SqlViewDataField"/> instance.</returns>
    [Pure]
    protected abstract SqlViewDataField CreateDataField(string name);

    [Pure]
    ISqlViewDataField ISqlViewDataFieldCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlViewDataField? ISqlViewDataFieldCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    IEnumerator<ISqlViewDataField> IEnumerable<ISqlViewDataField>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
