using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlIndexBuilderCollection : ISqlIndexBuilderCollection
{
    private readonly Dictionary<ReadOnlyMemory<ISqlIndexColumnBuilder>, MySqlIndexBuilder> _map;

    internal MySqlIndexBuilderCollection(MySqlTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<ReadOnlyMemory<ISqlIndexColumnBuilder>, MySqlIndexBuilder>(
            new MemoryElementWiseComparer<ISqlIndexColumnBuilder>() );
    }

    public MySqlTableBuilder Table { get; }
    public int Count => _map.Count;
    internal IEqualityComparer<ReadOnlyMemory<ISqlIndexColumnBuilder>> Comparer => _map.Comparer;

    ISqlTableBuilder ISqlIndexBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return _map.ContainsKey( columns );
    }

    [Pure]
    public MySqlIndexBuilder Get(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return _map[columns];
    }

    public bool TryGet(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, [MaybeNullWhen( false )] out MySqlIndexBuilder result)
    {
        return _map.TryGetValue( columns, out result );
    }

    public MySqlIndexBuilder Create(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        Table.EnsureNotRemoved();

        if ( Contains( columns ) )
            throw new MySqlObjectBuilderException( ExceptionResources.IndexAlreadyExists( columns ) );

        var indexColumns = MySqlHelpers.CreateIndexColumns( Table, columns );
        var result = Table.Schema.Objects.CreateIndex( Table, indexColumns, isUnique: false );
        _map.Add( indexColumns, result );
        return result;
    }

    public MySqlIndexBuilder GetOrCreate(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        Table.EnsureNotRemoved();

        if ( TryGet( columns, out var index ) )
            return index;

        var indexColumns = MySqlHelpers.CreateIndexColumns( Table, columns );
        var result = Table.Schema.Objects.CreateIndex( Table, indexColumns, isUnique: false );
        _map.Add( indexColumns, result );
        return result;
    }

    public bool Remove(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        if ( ! _map.TryGetValue( columns, out var index ) || ! index.CanRemove )
            return false;

        _map.Remove( columns );
        index.Remove();
        return true;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlIndexBuilder>
    {
        private Dictionary<ReadOnlyMemory<ISqlIndexColumnBuilder>, MySqlIndexBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<ReadOnlyMemory<ISqlIndexColumnBuilder>, MySqlIndexBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlIndexBuilder Current => _enumerator.Current;
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

    internal void ClearInto(RentedMemorySequenceSpan<MySqlObjectBuilder> buffer)
    {
        _map.Values.CopyTo( buffer );
        _map.Clear();
    }

    internal void Clear()
    {
        _map.Clear();
    }

    internal MySqlIndexBuilder GetOrCreateForPrimaryKey(MySqlIndexColumnBuilder[] columns)
    {
        if ( TryGet( columns, out var index ) )
            return index.MarkAsUnique().SetFilter( null );

        var result = Table.Schema.Objects.CreateIndex( Table, columns, isUnique: true );
        _map.Add( columns, result );
        return result;
    }

    [Pure]
    ISqlIndexBuilder ISqlIndexBuilderCollection.Get(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return Get( columns );
    }

    bool ISqlIndexBuilderCollection.TryGet(
        ReadOnlyMemory<ISqlIndexColumnBuilder> columns,
        [MaybeNullWhen( false )] out ISqlIndexBuilder result)
    {
        if ( TryGet( columns, out var index ) )
        {
            result = index;
            return true;
        }

        result = null;
        return false;
    }

    ISqlIndexBuilder ISqlIndexBuilderCollection.Create(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return Create( columns );
    }

    ISqlIndexBuilder ISqlIndexBuilderCollection.GetOrCreate(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return GetOrCreate( columns );
    }

    IEnumerator<ISqlIndexBuilder> IEnumerable<ISqlIndexBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
