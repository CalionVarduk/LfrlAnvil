using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlObjectCollection : ISqlObjectCollection
{
    private readonly Dictionary<string, MySqlObject> _map;

    internal MySqlObjectCollection(MySqlSchema schema, int count)
    {
        Schema = schema;
        _map = new Dictionary<string, MySqlObject>( capacity: count, comparer: StringComparer.OrdinalIgnoreCase );
    }

    public MySqlSchema Schema { get; }
    public int Count => _map.Count;

    ISqlSchema ISqlObjectCollection.Schema => Schema;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlObject Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlObject? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public MySqlTable GetTable(string name)
    {
        return GetTypedObject<MySqlTable>( name, SqlObjectType.Table );
    }

    [Pure]
    public MySqlTable? TryGetTable(string name)
    {
        return TryGetTypedObject<MySqlTable>( name, SqlObjectType.Table );
    }

    [Pure]
    public MySqlIndex GetIndex(string name)
    {
        return GetTypedObject<MySqlIndex>( name, SqlObjectType.Index );
    }

    [Pure]
    public MySqlIndex? TryGetIndex(string name)
    {
        return TryGetTypedObject<MySqlIndex>( name, SqlObjectType.Index );
    }

    [Pure]
    public MySqlPrimaryKey GetPrimaryKey(string name)
    {
        return GetTypedObject<MySqlPrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public MySqlPrimaryKey? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<MySqlPrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public MySqlForeignKey GetForeignKey(string name)
    {
        return GetTypedObject<MySqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public MySqlForeignKey? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<MySqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public MySqlCheck GetCheck(string name)
    {
        return GetTypedObject<MySqlCheck>( name, SqlObjectType.Check );
    }

    [Pure]
    public MySqlCheck? TryGetCheck(string name)
    {
        return TryGetTypedObject<MySqlCheck>( name, SqlObjectType.Check );
    }

    [Pure]
    public MySqlView GetView(string name)
    {
        return GetTypedObject<MySqlView>( name, SqlObjectType.View );
    }

    [Pure]
    public MySqlView? TryGetView(string name)
    {
        return TryGetTypedObject<MySqlView>( name, SqlObjectType.View );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlObject>
    {
        private Dictionary<string, MySqlObject>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlObject> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlObject Current => _enumerator.Current;
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

    internal void AddConstraintsWithoutForeignKeys(
        MySqlObjectBuilderCollection objects,
        RentedMemorySequence<MySqlObjectBuilder> foreignKeys)
    {
        foreach ( var b in objects )
        {
            switch ( b.Type )
            {
                case SqlObjectType.Table:
                {
                    var builder = ReinterpretCast.To<MySqlTableBuilder>( b );
                    var table = new MySqlTable( Schema, builder );
                    _map.Add( table.Name, table );

                    foreach ( var cb in builder.Constraints )
                    {
                        switch ( cb.Type )
                        {
                            case SqlObjectType.Index:
                            {
                                var index = table.Constraints.AddIndex( ReinterpretCast.To<MySqlIndexBuilder>( cb ) );
                                _map.Add( index.Name, index );
                                break;
                            }
                            case SqlObjectType.Check:
                            {
                                var check = table.Constraints.AddCheck( ReinterpretCast.To<MySqlCheckBuilder>( cb ) );
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
                    var view = new MySqlView( Schema, ReinterpretCast.To<MySqlViewBuilder>( b ) );
                    _map.Add( view.Name, view );
                    break;
                }
            }
        }
    }

    internal void AddForeignKey(MySqlForeignKeyBuilder builder, MySqlSchema referencedSchema)
    {
        var table = ReinterpretCast.To<MySqlTable>( _map[builder.Table.Name] );
        var foreignKey = table.Constraints.AddForeignKey(
            ReinterpretCast.To<MySqlIndex>( _map[builder.OriginIndex.Name] ),
            ReinterpretCast.To<MySqlIndex>( referencedSchema.Objects._map[builder.ReferencedIndex.Name] ),
            builder );

        _map.Add( foreignKey.Name, foreignKey );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlObject
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlObject
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
