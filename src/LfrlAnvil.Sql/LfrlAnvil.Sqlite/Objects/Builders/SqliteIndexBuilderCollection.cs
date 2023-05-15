using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteIndexBuilderCollection : ISqlIndexBuilderCollection
{
    private readonly Dictionary<ReadOnlyMemory<ISqlIndexColumnBuilder>, SqliteIndexBuilder> _map;

    internal SqliteIndexBuilderCollection(SqliteTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<ReadOnlyMemory<ISqlIndexColumnBuilder>, SqliteIndexBuilder>(
            new MemoryElementWiseComparer<ISqlIndexColumnBuilder>() );
    }

    public SqliteTableBuilder Table { get; }
    public int Count => _map.Count;
    internal IEqualityComparer<ReadOnlyMemory<ISqlIndexColumnBuilder>> Comparer => _map.Comparer;

    ISqlTableBuilder ISqlIndexBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return _map.ContainsKey( columns );
    }

    [Pure]
    public SqliteIndexBuilder Get(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return _map[columns];
    }

    public bool TryGet(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, [MaybeNullWhen( false )] out SqliteIndexBuilder result)
    {
        return _map.TryGetValue( columns, out result );
    }

    public SqliteIndexBuilder Create(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        Table.EnsureNotRemoved();

        if ( Contains( columns ) )
            throw new SqliteObjectBuilderException( ExceptionResources.IndexAlreadyExists( columns ) );

        var indexColumns = SqliteHelpers.CreateIndexColumns( Table, columns );
        var result = Table.Schema.Objects.CreateIndex( Table, indexColumns, isUnique: false );
        _map.Add( indexColumns, result );
        return result;
    }

    public SqliteIndexBuilder GetOrCreate(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        Table.EnsureNotRemoved();

        if ( TryGet( columns, out var index ) )
            return index;

        var indexColumns = SqliteHelpers.CreateIndexColumns( Table, columns );
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
    public IReadOnlyCollection<SqliteIndexBuilder> AsCollection()
    {
        return _map.Values;
    }

    [Pure]
    public IEnumerator<SqliteIndexBuilder> GetEnumerator()
    {
        return AsCollection().GetEnumerator();
    }

    internal void ClearInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _map.Values.CopyTo( buffer );
        _map.Clear();
    }

    internal SqliteIndexBuilder GetOrCreateForPrimaryKey(SqliteIndexColumnBuilder[] columns)
    {
        if ( TryGet( columns, out var index ) )
            return index.MarkAsUnique();

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
