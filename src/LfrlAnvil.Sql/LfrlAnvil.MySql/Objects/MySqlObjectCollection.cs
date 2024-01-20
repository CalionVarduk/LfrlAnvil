using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

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

    public bool TryGet(string name, [MaybeNullWhen( false )] out MySqlObject result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public MySqlTable GetTable(string name)
    {
        return GetTypedObject<MySqlTable>( name, SqlObjectType.Table );
    }

    public bool TryGetTable(string name, [MaybeNullWhen( false )] out MySqlTable result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    public MySqlIndex GetIndex(string name)
    {
        return GetTypedObject<MySqlIndex>( name, SqlObjectType.Index );
    }

    public bool TryGetIndex(string name, [MaybeNullWhen( false )] out MySqlIndex result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    public MySqlPrimaryKey GetPrimaryKey(string name)
    {
        return GetTypedObject<MySqlPrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    public bool TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out MySqlPrimaryKey result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    public MySqlForeignKey GetForeignKey(string name)
    {
        return GetTypedObject<MySqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    public bool TryGetForeignKey(string name, [MaybeNullWhen( false )] out MySqlForeignKey result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    [Pure]
    public MySqlCheck GetCheck(string name)
    {
        return GetTypedObject<MySqlCheck>( name, SqlObjectType.Check );
    }

    public bool TryGetCheck(string name, [MaybeNullWhen( false )] out MySqlCheck result)
    {
        return TryGetTypedObject( name, SqlObjectType.Check, out result );
    }

    [Pure]
    public MySqlView GetView(string name)
    {
        return GetTypedObject<MySqlView>( name, SqlObjectType.View );
    }

    public bool TryGetView(string name, [MaybeNullWhen( false )] out MySqlView result)
    {
        return TryGetTypedObject( name, SqlObjectType.View, out result );
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

    internal void Populate(MySqlObjectBuilderCollection objects, RentedMemorySequence<MySqlObjectBuilder> tables)
    {
        foreach ( var b in objects )
        {
            switch ( b.Type )
            {
                case SqlObjectType.Table:
                {
                    tables.Push( b );
                    var tableBuilder = ReinterpretCast.To<MySqlTableBuilder>( b );
                    var table = new MySqlTable( Schema, tableBuilder );
                    _map.Add( table.Name, table );

                    foreach ( var ix in table.Indexes )
                        _map.Add( ix.Name, ix );

                    foreach ( var chk in table.Checks )
                        _map.Add( chk.Name, chk );

                    table.SetPrimaryKey( tableBuilder );
                    _map.Add( table.PrimaryKey.Name, table.PrimaryKey );
                    break;
                }
                case SqlObjectType.View:
                {
                    var viewBuilder = ReinterpretCast.To<MySqlViewBuilder>( b );
                    var view = new MySqlView( Schema, viewBuilder );
                    _map.Add( view.Name, view );
                    break;
                }
            }
        }
    }

    internal void Populate(MySqlForeignKeyCollection foreignKeys)
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
            throw new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() );

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
    ISqlCheck ISqlObjectCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    bool ISqlObjectCollection.TryGetCheck(string name, [MaybeNullWhen( false )] out ISqlCheck result)
    {
        return TryGetTypedObject( name, SqlObjectType.Check, out result );
    }

    [Pure]
    ISqlView ISqlObjectCollection.GetView(string name)
    {
        return GetView( name );
    }

    bool ISqlObjectCollection.TryGetView(string name, [MaybeNullWhen( false )] out ISqlView result)
    {
        return TryGetTypedObject( name, SqlObjectType.View, out result );
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
