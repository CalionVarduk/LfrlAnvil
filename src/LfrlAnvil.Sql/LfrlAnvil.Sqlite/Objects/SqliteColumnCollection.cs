using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteColumnCollection : ISqlColumnCollection
{
    private readonly Dictionary<string, SqliteColumn> _map;

    internal SqliteColumnCollection(SqliteTable table, SqliteColumnBuilderCollection columns)
    {
        Table = table;

        _map = new Dictionary<string, SqliteColumn>( capacity: columns.Count, comparer: StringComparer.OrdinalIgnoreCase );
        foreach ( var b in columns )
            _map.Add( b.Name, new SqliteColumn( table, b ) );
    }

    public SqliteTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlColumnCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteColumn GetColumn(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqliteColumn? TryGetColumn(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteColumn>
    {
        private Dictionary<string, SqliteColumn>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteColumn> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteColumn Current => _enumerator.Current;
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
