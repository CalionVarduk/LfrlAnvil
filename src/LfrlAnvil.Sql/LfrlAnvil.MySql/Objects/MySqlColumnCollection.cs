using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlColumnCollection : ISqlColumnCollection
{
    private readonly Dictionary<string, MySqlColumn> _map;

    internal MySqlColumnCollection(MySqlTable table, MySqlColumnBuilderCollection columns)
    {
        Table = table;

        _map = new Dictionary<string, MySqlColumn>( capacity: columns.Count, comparer: StringComparer.OrdinalIgnoreCase );
        foreach ( var b in columns )
            _map.Add( b.Name, new MySqlColumn( table, b ) );
    }

    public MySqlTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlColumnCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlColumn GetColumn(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlColumn? TryGetColumn(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlColumn>
    {
        private Dictionary<string, MySqlColumn>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlColumn> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlColumn Current => _enumerator.Current;
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
    ISqlColumn ISqlColumnCollection.GetColumn(string name)
    {
        return GetColumn( name );
    }

    [Pure]
    ISqlColumn? ISqlColumnCollection.TryGetColumn(string name)
    {
        return TryGetColumn( name );
    }

    [Pure]
    IEnumerator<ISqlColumn> IEnumerable<ISqlColumn>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
