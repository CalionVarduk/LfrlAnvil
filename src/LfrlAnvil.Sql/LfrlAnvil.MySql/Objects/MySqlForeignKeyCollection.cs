using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlForeignKeyCollection : ISqlForeignKeyCollection
{
    private readonly Dictionary<Pair<ISqlIndex, ISqlIndex>, MySqlForeignKey> _map;

    internal MySqlForeignKeyCollection(MySqlTable table, int count)
    {
        Table = table;
        _map = new Dictionary<Pair<ISqlIndex, ISqlIndex>, MySqlForeignKey>( capacity: count );
    }

    public MySqlTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlForeignKeyCollection.Table => Table;

    [Pure]
    public bool Contains(ISqlIndex originIndex, ISqlIndex referencedIndex)
    {
        return _map.ContainsKey( Pair.Create( originIndex, referencedIndex ) );
    }

    [Pure]
    public MySqlForeignKey Get(ISqlIndex originIndex, ISqlIndex referencedIndex)
    {
        return _map[Pair.Create( originIndex, referencedIndex )];
    }

    public bool TryGet(
        ISqlIndex originIndex,
        ISqlIndex referencedIndex,
        [MaybeNullWhen( false )] out MySqlForeignKey result)
    {
        return _map.TryGetValue( Pair.Create( originIndex, referencedIndex ), out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlForeignKey>
    {
        private Dictionary<Pair<ISqlIndex, ISqlIndex>, MySqlForeignKey>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<Pair<ISqlIndex, ISqlIndex>, MySqlForeignKey> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlForeignKey Current => _enumerator.Current;
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

    internal void Populate(MySqlSchemaCollection schemas, MySqlForeignKeyBuilderCollection foreignKeys)
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
                new MySqlForeignKey( index, refIndex, b ) );
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
