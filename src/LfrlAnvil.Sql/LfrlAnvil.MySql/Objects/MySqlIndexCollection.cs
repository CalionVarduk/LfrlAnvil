using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlIndexCollection : ISqlIndexCollection
{
    private readonly Dictionary<ReadOnlyMemory<ISqlIndexColumn>, MySqlIndex> _map;

    internal MySqlIndexCollection(MySqlTable table, MySqlIndexBuilderCollection indexes)
    {
        Table = table;

        _map = new Dictionary<ReadOnlyMemory<ISqlIndexColumn>, MySqlIndex>(
            capacity: indexes.Count,
            comparer: new MemoryElementWiseComparer<ISqlIndexColumn>() );

        foreach ( var b in indexes )
        {
            var ix = new MySqlIndex( table, b );
            _map.Add( ((ISqlIndex)ix).Columns, ix );
        }
    }

    public MySqlTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlIndexCollection.Table => Table;

    [Pure]
    public bool Contains(ReadOnlyMemory<ISqlIndexColumn> columns)
    {
        return _map.ContainsKey( columns );
    }

    [Pure]
    public MySqlIndex Get(ReadOnlyMemory<ISqlIndexColumn> columns)
    {
        return _map[columns];
    }

    public bool TryGet(ReadOnlyMemory<ISqlIndexColumn> columns, [MaybeNullWhen( false )] out MySqlIndex result)
    {
        return _map.TryGetValue( columns, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlIndex>
    {
        private Dictionary<ReadOnlyMemory<ISqlIndexColumn>, MySqlIndex>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<ReadOnlyMemory<ISqlIndexColumn>, MySqlIndex> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlIndex Current => _enumerator.Current;
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
