using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteViewDataFieldCollection : ISqlViewDataFieldCollection
{
    private readonly Dictionary<string, SqliteViewDataField> _map;

    internal SqliteViewDataFieldCollection(SqliteView view, SqlQueryExpressionNode source)
    {
        View = view;
        _map = new Dictionary<string, SqliteViewDataField>(
            capacity: source.Selection.Length,
            comparer: StringComparer.OrdinalIgnoreCase );

        source.ReduceKnownDataFieldExpressions( e => _map.Add( e.Key, new SqliteViewDataField( view, e.Key ) ) );
    }

    public SqliteView View { get; }
    public int Count => _map.Count;

    ISqlView ISqlViewDataFieldCollection.View => View;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteViewDataField Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqliteViewDataField? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteViewDataField>
    {
        private Dictionary<string, SqliteViewDataField>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteViewDataField> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteViewDataField Current => _enumerator.Current;
        object IEnumerator.Current => Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }
    }

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
