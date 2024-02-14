using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlViewDataFieldCollection : ISqlViewDataFieldCollection
{
    private readonly Dictionary<string, SqlViewDataField> _map;
    private SqlView? _view;

    protected SqlViewDataFieldCollection(SqlQueryExpressionNode source)
    {
        _map = new Dictionary<string, SqlViewDataField>( capacity: source.Selection.Length, comparer: SqlHelpers.NameComparer );
        _view = null;
    }

    public int Count => _map.Count;

    public SqlView View
    {
        get
        {
            Assume.IsNotNull( _view );
            return _view;
        }
    }

    ISqlView ISqlViewDataFieldCollection.View => View;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlViewDataField Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlViewDataField? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

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
