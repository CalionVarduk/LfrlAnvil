using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteConstraintCollection : ISqlConstraintCollection
{
    private readonly Dictionary<string, SqliteConstraint> _map;
    private SqlitePrimaryKey? _primaryKey;

    internal SqliteConstraintCollection(SqliteTable table, int count)
    {
        Table = table;
        _map = new Dictionary<string, SqliteConstraint>( capacity: count, comparer: StringComparer.OrdinalIgnoreCase );
        _primaryKey = null;
    }

    public SqliteTable Table { get; }

    public SqlitePrimaryKey PrimaryKey
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
    public SqliteConstraint GetConstraint(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqliteConstraint? TryGetConstraint(string name)
    {
        return _map.GetValueOrDefault( name );
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
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteConstraint>
    {
        private Dictionary<string, SqliteConstraint>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteConstraint> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteConstraint Current => _enumerator.Current;
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
    internal SqliteIndex AddIndex(SqliteIndexBuilder builder)
    {
        var index = new SqliteIndex( Table, builder );
        _map.Add( index.Name, index );
        return index;
    }

    [Pure]
    internal SqliteCheck AddCheck(SqliteCheckBuilder builder)
    {
        var check = new SqliteCheck( Table, builder );
        _map.Add( check.Name, check );
        return check;
    }

    [Pure]
    internal SqliteForeignKey AddForeignKey(SqliteIndex originIndex, SqliteIndex referencedIndex, SqliteForeignKeyBuilder builder)
    {
        var foreignKey = new SqliteForeignKey( originIndex, referencedIndex, builder );
        _map.Add( foreignKey.Name, foreignKey );
        return foreignKey;
    }

    [Pure]
    internal SqlitePrimaryKey SetPrimaryKey(SqliteConstraintBuilderCollection constraints)
    {
        Assume.IsNull( _primaryKey );

        var primaryKey = constraints.TryGetPrimaryKey();
        if ( primaryKey is null )
            throw new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( constraints.Table ) );

        var index = ReinterpretCast.To<SqliteIndex>( _map[primaryKey.Index.Name] );
        _primaryKey = new SqlitePrimaryKey( index, primaryKey );
        _map.Add( _primaryKey.Name, _primaryKey );
        return _primaryKey;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteConstraint
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( SqliteDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteConstraint
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlConstraint ISqlConstraintCollection.GetConstraint(string name)
    {
        return GetConstraint( name );
    }

    [Pure]
    ISqlConstraint? ISqlConstraintCollection.TryGetConstraint(string name)
    {
        return TryGetConstraint( name );
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
