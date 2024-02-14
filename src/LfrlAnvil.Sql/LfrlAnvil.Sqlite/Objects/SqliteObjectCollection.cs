using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects;
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

    [Pure]
    public SqliteObject? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqliteTable GetTable(string name)
    {
        return GetTypedObject<SqliteTable>( name, SqlObjectType.Table );
    }

    [Pure]
    public SqliteTable? TryGetTable(string name)
    {
        return TryGetTypedObject<SqliteTable>( name, SqlObjectType.Table );
    }

    [Pure]
    public SqliteIndex GetIndex(string name)
    {
        return GetTypedObject<SqliteIndex>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqliteIndex? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqliteIndex>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqlitePrimaryKey GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlitePrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public SqlitePrimaryKey? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<SqlitePrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public SqliteForeignKey GetForeignKey(string name)
    {
        return GetTypedObject<SqliteForeignKey>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqliteForeignKey? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqliteForeignKey>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqliteCheck GetCheck(string name)
    {
        return GetTypedObject<SqliteCheck>( name, SqlObjectType.Check );
    }

    [Pure]
    public SqliteCheck? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqliteCheck>( name, SqlObjectType.Check );
    }

    [Pure]
    public SqliteView GetView(string name)
    {
        return GetTypedObject<SqliteView>( name, SqlObjectType.View );
    }

    [Pure]
    public SqliteView? TryGetView(string name)
    {
        return TryGetTypedObject<SqliteView>( name, SqlObjectType.View );
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

    internal void AddConstraintsWithoutForeignKeys(SqliteObjectBuilderCollection objects, RentedMemorySequence<SqliteObjectBuilder> foreignKeys)
    {
        foreach ( var b in objects )
        {
            switch ( b.Type )
            {
                case SqlObjectType.Table:
                {
                    var builder = ReinterpretCast.To<SqliteTableBuilder>( b );
                    var table = new SqliteTable( Schema, builder );
                    _map.Add( table.Name, table );

                    foreach ( var cb in builder.Constraints )
                    {
                        switch ( cb.Type )
                        {
                            case SqlObjectType.Index:
                            {
                                var index = table.Constraints.AddIndex( ReinterpretCast.To<SqliteIndexBuilder>( cb ) );
                                _map.Add( index.Name, index );
                                break;
                            }
                            case SqlObjectType.Check:
                            {
                                var check = table.Constraints.AddCheck( ReinterpretCast.To<SqliteCheckBuilder>( cb ) );
                                _map.Add( check.Name, check );
                                break;
                            }
                            case SqlObjectType.ForeignKey:
                            {
                                foreignKeys.Push( cb );
                                break;
                            }
                        }
                    }

                    var pk = table.Constraints.SetPrimaryKey( builder.Constraints );
                    _map.Add( pk.Name, pk );
                    break;
                }
                case SqlObjectType.View:
                {
                    var view = new SqliteView( Schema, ReinterpretCast.To<SqliteViewBuilder>( b ) );
                    _map.Add( view.Name, view );
                    break;
                }
            }
        }
    }

    internal void AddForeignKey(SqliteForeignKeyBuilder builder, SqliteSchema referencedSchema)
    {
        var table = ReinterpretCast.To<SqliteTable>( _map[builder.Table.Name] );
        var foreignKey = table.Constraints.AddForeignKey(
            ReinterpretCast.To<SqliteIndex>( _map[builder.OriginIndex.Name] ),
            ReinterpretCast.To<SqliteIndex>( referencedSchema.Objects._map[builder.ReferencedIndex.Name] ),
            builder );

        _map.Add( foreignKey.Name, foreignKey );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteObject
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( SqliteDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteObject
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlObject ISqlObjectCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlObject? ISqlObjectCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    ISqlTable ISqlObjectCollection.GetTable(string name)
    {
        return GetTable( name );
    }

    [Pure]
    ISqlTable? ISqlObjectCollection.TryGetTable(string name)
    {
        return TryGetTable( name );
    }

    [Pure]
    ISqlIndex ISqlObjectCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndex? ISqlObjectCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlPrimaryKey ISqlObjectCollection.GetPrimaryKey(string name)
    {
        return GetPrimaryKey( name );
    }

    [Pure]
    ISqlPrimaryKey? ISqlObjectCollection.TryGetPrimaryKey(string name)
    {
        return TryGetPrimaryKey( name );
    }

    [Pure]
    ISqlForeignKey ISqlObjectCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKey? ISqlObjectCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheck ISqlObjectCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheck? ISqlObjectCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    [Pure]
    ISqlView ISqlObjectCollection.GetView(string name)
    {
        return GetView( name );
    }

    [Pure]
    ISqlView? ISqlObjectCollection.TryGetView(string name)
    {
        return TryGetView( name );
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
