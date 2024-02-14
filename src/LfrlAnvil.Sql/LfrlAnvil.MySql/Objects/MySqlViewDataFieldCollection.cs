using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlViewDataFieldCollection : ISqlViewDataFieldCollection
{
    private readonly Dictionary<string, MySqlViewDataField> _map;

    internal MySqlViewDataFieldCollection(MySqlView view, SqlQueryExpressionNode source)
    {
        View = view;
        _map = new Dictionary<string, MySqlViewDataField>(
            capacity: source.Selection.Length,
            comparer: StringComparer.OrdinalIgnoreCase );

        source.ReduceKnownDataFieldExpressions( e => _map.Add( e.Key, new MySqlViewDataField( view, e.Key ) ) );
    }

    public MySqlView View { get; }
    public int Count => _map.Count;

    ISqlView ISqlViewDataFieldCollection.View => View;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlViewDataField Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlViewDataField? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlViewDataField>
    {
        private Dictionary<string, MySqlViewDataField>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlViewDataField> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlViewDataField Current => _enumerator.Current;
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
