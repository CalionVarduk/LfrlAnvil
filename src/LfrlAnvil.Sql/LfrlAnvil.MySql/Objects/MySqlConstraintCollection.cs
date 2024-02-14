using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlConstraintCollection : ISqlConstraintCollection
{
    private readonly Dictionary<string, MySqlConstraint> _map;
    private MySqlPrimaryKey? _primaryKey;

    internal MySqlConstraintCollection(MySqlTable table, int count)
    {
        Table = table;
        _map = new Dictionary<string, MySqlConstraint>( capacity: count, comparer: StringComparer.OrdinalIgnoreCase );
        _primaryKey = null;
    }

    public MySqlTable Table { get; }

    public MySqlPrimaryKey PrimaryKey
    {
        get
        {
            Assume.IsNotNull( _primaryKey );
            return _primaryKey;
        }
    }

    public int Count => _map.Count;

    ISqlTable ISqlConstraintCollection.Table => Table;
    ISqlPrimaryKey ISqlConstraintCollection.PrimaryKey => PrimaryKey;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlConstraint Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlConstraint? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
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
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlConstraint>
    {
        private Dictionary<string, MySqlConstraint>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlConstraint> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlConstraint Current => _enumerator.Current;
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
    internal MySqlIndex AddIndex(MySqlIndexBuilder builder)
    {
        var index = new MySqlIndex( Table, builder );
        _map.Add( index.Name, index );
        return index;
    }

    [Pure]
    internal MySqlCheck AddCheck(MySqlCheckBuilder builder)
    {
        var check = new MySqlCheck( Table, builder );
        _map.Add( check.Name, check );
        return check;
    }

    [Pure]
    internal MySqlForeignKey AddForeignKey(MySqlIndex originIndex, MySqlIndex referencedIndex, MySqlForeignKeyBuilder builder)
    {
        var foreignKey = new MySqlForeignKey( originIndex, referencedIndex, builder );
        _map.Add( foreignKey.Name, foreignKey );
        return foreignKey;
    }

    [Pure]
    internal MySqlPrimaryKey SetPrimaryKey(MySqlConstraintBuilderCollection constraints)
    {
        Assume.IsNull( _primaryKey );

        var primaryKey = constraints.TryGetPrimaryKey();
        if ( primaryKey is null )
            throw new MySqlObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( constraints.Table ) );

        var index = ReinterpretCast.To<MySqlIndex>( _map[primaryKey.Index.Name] );
        _primaryKey = new MySqlPrimaryKey( index, primaryKey );
        _map.Add( _primaryKey.Name, _primaryKey );
        return _primaryKey;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlConstraint
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlConstraint
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlConstraint ISqlConstraintCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlConstraint? ISqlConstraintCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    ISqlIndex ISqlConstraintCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndex? ISqlConstraintCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlForeignKey ISqlConstraintCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKey? ISqlConstraintCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheck ISqlConstraintCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheck? ISqlConstraintCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    [Pure]
    IEnumerator<ISqlConstraint> IEnumerable<ISqlConstraint>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
