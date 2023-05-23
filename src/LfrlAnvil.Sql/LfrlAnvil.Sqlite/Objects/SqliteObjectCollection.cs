using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteObjectCollection : ISqlObjectCollection
{
    private readonly Dictionary<string, SqliteObject> _map;

    internal SqliteObjectCollection(SqliteSchema schema, int count)
    {
        Schema = schema;
        _map = new Dictionary<string, SqliteObject>( capacity: count, comparer: StringComparer.OrdinalIgnoreCase );
    }

    public SqliteSchema Schema { get; }
    public int Count => _map.Count;

    ISqlSchema ISqlObjectCollection.Schema => Schema;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteObject Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out SqliteObject result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public SqliteTable GetTable(string name)
    {
        return GetTypedObject<SqliteTable>( name, SqlObjectType.Table );
    }

    public bool TryGetTable(string name, [MaybeNullWhen( false )] out SqliteTable result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    public SqliteIndex GetIndex(string name)
    {
        return GetTypedObject<SqliteIndex>( name, SqlObjectType.Index );
    }

    public bool TryGetIndex(string name, [MaybeNullWhen( false )] out SqliteIndex result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    public SqlitePrimaryKey GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlitePrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    public bool TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out SqlitePrimaryKey result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    public SqliteForeignKey GetForeignKey(string name)
    {
        return GetTypedObject<SqliteForeignKey>( name, SqlObjectType.ForeignKey );
    }

    public bool TryGetForeignKey(string name, [MaybeNullWhen( false )] out SqliteForeignKey result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteObject>
    {
        private Dictionary<string, SqliteObject>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteObject> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteObject Current => _enumerator.Current;
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

    internal void Populate(SqliteObjectBuilderCollection objects, RentedMemorySequence<SqliteObjectBuilder> tables)
    {
        foreach ( var b in objects )
        {
            if ( b.Type != SqlObjectType.Table )
                continue;

            tables.Push( b );
            var tableBuilder = ReinterpretCast.To<SqliteTableBuilder>( b );
            var table = new SqliteTable( Schema, tableBuilder );
            _map.Add( table.Name, table );

            foreach ( var ix in table.Indexes )
                _map.Add( ix.Name, ix );

            table.SetPrimaryKey( tableBuilder );
            _map.Add( table.PrimaryKey.Name, table.PrimaryKey );
        }
    }

    internal void Populate(SqliteForeignKeyCollection foreignKeys)
    {
        foreach ( var fk in foreignKeys )
            _map.Add( fk.Name, fk );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : class, ISqlObject
    {
        var obj = _map[name];
        if ( obj.Type != type )
            throw new SqliteObjectCastException( typeof( T ), obj.GetType() );

        return ReinterpretCast.To<T>( obj );
    }

    private bool TryGetTypedObject<T>(string name, SqlObjectType type, [MaybeNullWhen( false )] out T result)
        where T : class, ISqlObject
    {
        if ( _map.TryGetValue( name, out var obj ) && obj.Type == type )
        {
            result = ReinterpretCast.To<T>( obj );
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    ISqlObject ISqlObjectCollection.Get(string name)
    {
        return Get( name );
    }

    bool ISqlObjectCollection.TryGet(string name, [MaybeNullWhen( false )] out ISqlObject result)
    {
        if ( TryGet( name, out var obj ) )
        {
            result = obj;
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    ISqlTable ISqlObjectCollection.GetTable(string name)
    {
        return GetTable( name );
    }

    bool ISqlObjectCollection.TryGetTable(string name, [MaybeNullWhen( false )] out ISqlTable result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    ISqlIndex ISqlObjectCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    bool ISqlObjectCollection.TryGetIndex(string name, [MaybeNullWhen( false )] out ISqlIndex result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    ISqlPrimaryKey ISqlObjectCollection.GetPrimaryKey(string name)
    {
        return GetPrimaryKey( name );
    }

    bool ISqlObjectCollection.TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out ISqlPrimaryKey result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    ISqlForeignKey ISqlObjectCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    bool ISqlObjectCollection.TryGetForeignKey(string name, [MaybeNullWhen( false )] out ISqlForeignKey result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    [Pure]
    IEnumerator<ISqlObject> IEnumerable<ISqlObject>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
