using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteForeignKeyCollection : ISqlForeignKeyCollection
{
    private readonly Dictionary<Pair<ISqlIndex, ISqlIndex>, SqliteForeignKey> _map;

    internal SqliteForeignKeyCollection(SqliteTable table, int count)
    {
        Table = table;
        _map = new Dictionary<Pair<ISqlIndex, ISqlIndex>, SqliteForeignKey>( capacity: count );
    }

    public SqliteTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlForeignKeyCollection.Table => Table;

    [Pure]
    public bool Contains(ISqlIndex originIndex, ISqlIndex referencedIndex)
    {
        return _map.ContainsKey( Pair.Create( originIndex, referencedIndex ) );
    }

    [Pure]
    public SqliteForeignKey Get(ISqlIndex originIndex, ISqlIndex referencedIndex)
    {
        return _map[Pair.Create( originIndex, referencedIndex )];
    }

    public bool TryGet(
        ISqlIndex originIndex,
        ISqlIndex referencedIndex,
        [MaybeNullWhen( false )] out SqliteForeignKey result)
    {
        return _map.TryGetValue( Pair.Create( originIndex, referencedIndex ), out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteForeignKey>
    {
        private Dictionary<Pair<ISqlIndex, ISqlIndex>, SqliteForeignKey>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<Pair<ISqlIndex, ISqlIndex>, SqliteForeignKey> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteForeignKey Current => _enumerator.Current;
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

    internal void Populate(SqliteSchemaCollection schemas, SqliteForeignKeyBuilderCollection foreignKeys)
    {
        foreach ( var b in foreignKeys )
        {
            var indexSchemaBuilder = b.OriginIndex.Table.Schema;
            var schema = schemas.Get( indexSchemaBuilder.Name );
            var index = schema.Objects.GetIndex( b.OriginIndex.Name );

            var refIndexSchemaBuilder = b.ReferencedIndex.Table.Schema;
            if ( ! ReferenceEquals( indexSchemaBuilder, refIndexSchemaBuilder ) )
                schema = schemas.Get( refIndexSchemaBuilder.Name );

            var refIndex = schema.Objects.GetIndex( b.ReferencedIndex.Name );

            _map.Add(
                Pair.Create<ISqlIndex, ISqlIndex>( index, refIndex ),
                new SqliteForeignKey( index, refIndex, b ) );
        }
    }

    [Pure]
    ISqlForeignKey ISqlForeignKeyCollection.Get(ISqlIndex originIndex, ISqlIndex referencedIndex)
    {
        return Get( originIndex, referencedIndex );
    }

    bool ISqlForeignKeyCollection.TryGet(
        ISqlIndex originIndex,
        ISqlIndex referencedIndex,
        [MaybeNullWhen( false )] out ISqlForeignKey result)
    {
        if ( TryGet( originIndex, referencedIndex, out var fk ) )
        {
            result = fk;
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    IEnumerator<ISqlForeignKey> IEnumerable<ISqlForeignKey>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
