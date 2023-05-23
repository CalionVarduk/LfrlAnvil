using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteForeignKeyBuilderCollection : ISqlForeignKeyBuilderCollection
{
    private readonly Dictionary<Pair<ISqlIndexBuilder, ISqlIndexBuilder>, SqliteForeignKeyBuilder> _map;

    internal SqliteForeignKeyBuilderCollection(SqliteTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<Pair<ISqlIndexBuilder, ISqlIndexBuilder>, SqliteForeignKeyBuilder>();
    }

    public SqliteTableBuilder Table { get; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlForeignKeyBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex)
    {
        return _map.ContainsKey( Pair.Create( index, referencedIndex ) );
    }

    [Pure]
    public SqliteForeignKeyBuilder Get(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex)
    {
        return _map[Pair.Create( index, referencedIndex )];
    }

    public bool TryGet(
        ISqlIndexBuilder index,
        ISqlIndexBuilder referencedIndex,
        [MaybeNullWhen( false )] out SqliteForeignKeyBuilder result)
    {
        return _map.TryGetValue( Pair.Create( index, referencedIndex ), out result );
    }

    public SqliteForeignKeyBuilder Create(SqliteIndexBuilder index, SqliteIndexBuilder referencedIndex)
    {
        Table.EnsureNotRemoved();

        var key = Pair.Create<ISqlIndexBuilder, ISqlIndexBuilder>( index, referencedIndex );
        if ( _map.ContainsKey( key ) )
            throw new SqliteObjectBuilderException( ExceptionResources.ForeignKeyAlreadyExists( index, referencedIndex ) );

        var foreignKey = Table.Schema.Objects.CreateForeignKey( Table, index, referencedIndex );
        _map.Add( key, foreignKey );
        return foreignKey;
    }

    public SqliteForeignKeyBuilder GetOrCreate(SqliteIndexBuilder index, SqliteIndexBuilder referencedIndex)
    {
        Table.EnsureNotRemoved();

        var key = Pair.Create<ISqlIndexBuilder, ISqlIndexBuilder>( index, referencedIndex );
        if ( _map.TryGetValue( key, out var foreignKey ) )
            return foreignKey;

        foreignKey = Table.Schema.Objects.CreateForeignKey( Table, index, referencedIndex );
        _map.Add( key, foreignKey );
        return foreignKey;
    }

    public bool Remove(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex)
    {
        if ( ! _map.Remove( Pair.Create( index, referencedIndex ), out var removed ) )
            return false;

        removed.Remove();
        return true;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteForeignKeyBuilder>
    {
        private Dictionary<Pair<ISqlIndexBuilder, ISqlIndexBuilder>, SqliteForeignKeyBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<Pair<ISqlIndexBuilder, ISqlIndexBuilder>, SqliteForeignKeyBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteForeignKeyBuilder Current => _enumerator.Current;
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

    internal void Clear()
    {
        _map.Clear();
    }

    internal void Reactivate(SqliteForeignKeyBuilder foreignKey)
    {
        _map.Add( Pair.Create<ISqlIndexBuilder, ISqlIndexBuilder>( foreignKey.Index, foreignKey.ReferencedIndex ), foreignKey );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlForeignKeyBuilderCollection.Get(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex)
    {
        return Get( index, referencedIndex );
    }

    bool ISqlForeignKeyBuilderCollection.TryGet(
        ISqlIndexBuilder index,
        ISqlIndexBuilder referencedIndex,
        [MaybeNullWhen( false )] out ISqlForeignKeyBuilder result)
    {
        if ( TryGet( index, referencedIndex, out var fk ) )
        {
            result = fk;
            return true;
        }

        result = null;
        return false;
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilderCollection.Create(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex)
    {
        return Create(
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( index ),
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( referencedIndex ) );
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilderCollection.GetOrCreate(ISqlIndexBuilder index, ISqlIndexBuilder referencedIndex)
    {
        return GetOrCreate(
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( index ),
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( referencedIndex ) );
    }

    [Pure]
    IEnumerator<ISqlForeignKeyBuilder> IEnumerable<ISqlForeignKeyBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
