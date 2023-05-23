using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteIndexCollection : ISqlIndexCollection
{
    private readonly Dictionary<ReadOnlyMemory<ISqlIndexColumn>, SqliteIndex> _map;

    internal SqliteIndexCollection(SqliteTable table, SqliteIndexBuilderCollection indexes)
    {
        Table = table;

        _map = new Dictionary<ReadOnlyMemory<ISqlIndexColumn>, SqliteIndex>(
            capacity: indexes.Count,
            comparer: new MemoryElementWiseComparer<ISqlIndexColumn>() );

        foreach ( var b in indexes )
        {
            var ix = new SqliteIndex( table, b );
            _map.Add( ((ISqlIndex)ix).Columns, ix );
        }
    }

    public SqliteTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlIndexCollection.Table => Table;

    [Pure]
    public bool Contains(ReadOnlyMemory<ISqlIndexColumn> columns)
    {
        return _map.ContainsKey( columns );
    }

    [Pure]
    public SqliteIndex Get(ReadOnlyMemory<ISqlIndexColumn> columns)
    {
        return _map[columns];
    }

    public bool TryGet(ReadOnlyMemory<ISqlIndexColumn> columns, [MaybeNullWhen( false )] out SqliteIndex result)
    {
        return _map.TryGetValue( columns, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteIndex>
    {
        private Dictionary<ReadOnlyMemory<ISqlIndexColumn>, SqliteIndex>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<ReadOnlyMemory<ISqlIndexColumn>, SqliteIndex> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteIndex Current => _enumerator.Current;
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
    ISqlIndex ISqlIndexCollection.Get(ReadOnlyMemory<ISqlIndexColumn> columns)
    {
        return Get( columns );
    }

    bool ISqlIndexCollection.TryGet(
        ReadOnlyMemory<ISqlIndexColumn> columns,
        [MaybeNullWhen( false )] out ISqlIndex result)
    {
        if ( TryGet( columns, out var index ) )
        {
            result = index;
            return true;
        }

        result = null;
        return false;
    }

    IEnumerator<ISqlIndex> IEnumerable<ISqlIndex>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
