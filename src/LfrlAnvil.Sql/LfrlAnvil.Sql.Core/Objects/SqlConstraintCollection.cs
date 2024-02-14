using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlConstraintCollection : ISqlConstraintCollection
{
    private readonly Dictionary<string, SqlConstraint> _map;
    private SqlTable? _table;
    private SqlPrimaryKey? _primaryKey;

    protected SqlConstraintCollection(SqlConstraintBuilderCollection source)
    {
        _map = new Dictionary<string, SqlConstraint>( capacity: source.Count, comparer: SqlHelpers.NameComparer );
        _table = null;
        _primaryKey = null;
    }

    public int Count => _map.Count;

    public SqlTable Table
    {
        get
        {
            Assume.IsNotNull( _table );
            return _table;
        }
    }

    public SqlPrimaryKey PrimaryKey
    {
        get
        {
            Assume.IsNotNull( _primaryKey );
            return _primaryKey;
        }
    }

    ISqlTable ISqlConstraintCollection.Table => Table;
    ISqlPrimaryKey ISqlConstraintCollection.PrimaryKey => PrimaryKey;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlConstraint Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlConstraint? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqlIndex GetIndex(string name)
    {
        return GetTypedObject<SqlIndex>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqlIndex? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqlIndex>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqlForeignKey GetForeignKey(string name)
    {
        return GetTypedObject<SqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqlForeignKey? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqlCheck GetCheck(string name)
    {
        return GetTypedObject<SqlCheck>( name, SqlObjectType.Check );
    }

    [Pure]
    public SqlCheck? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqlCheck>( name, SqlObjectType.Check );
    }

    [Pure]
    public SqlObjectEnumerator<SqlConstraint> GetEnumerator()
    {
        return new SqlObjectEnumerator<SqlConstraint>( _map );
    }

    internal void SetTable(SqlTable table)
    {
        Assume.IsNull( _table );
        Assume.Equals( table.Constraints, this );
        _table = table;
    }

    internal void AddConstraint(SqlConstraint constraint)
    {
        Assume.NotEquals( constraint.Type, SqlObjectType.PrimaryKey );
        _map.Add( constraint.Name, constraint );
    }

    internal void SetPrimaryKey(SqlPrimaryKey primaryKey)
    {
        Assume.IsNull( _primaryKey );
        Assume.Equals( primaryKey.Table, Table );
        _primaryKey = primaryKey;
        _map.Add( primaryKey.Name, primaryKey );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlConstraint
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw SqlHelpers.CreateObjectCastException( Table.Database, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlConstraint
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
