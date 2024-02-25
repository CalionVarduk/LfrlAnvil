using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlConstraintBuilderCollection : SqlBuilderApi, ISqlConstraintBuilderCollection
{
    private readonly Dictionary<string, SqlConstraintBuilder> _map;
    private SqlTableBuilder? _table;
    private SqlPrimaryKeyBuilder? _primaryKey;

    protected SqlConstraintBuilderCollection()
    {
        _table = null;
        _primaryKey = null;
        _map = new Dictionary<string, SqlConstraintBuilder>( SqlHelpers.NameComparer );
    }

    public SqlTableBuilder Table
    {
        get
        {
            Assume.IsNotNull( _table );
            return _table;
        }
    }

    public int Count => _map.Count;

    ISqlTableBuilder ISqlConstraintBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlConstraintBuilder Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlConstraintBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return TryGetPrimaryKey() ??
            throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.PrimaryKeyIsMissing( Table ) );
    }

    [Pure]
    public SqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return _primaryKey;
    }

    [Pure]
    public SqlIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<SqlIndexBuilder>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqlIndexBuilder? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqlIndexBuilder>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqlForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<SqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqlCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<SqlCheckBuilder>( name, SqlObjectType.Check );
    }

    [Pure]
    public SqlCheckBuilder? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqlCheckBuilder>( name, SqlObjectType.Check );
    }

    public SqlPrimaryKeyBuilder SetPrimaryKey(SqlIndexBuilder index)
    {
        return SetPrimaryKey( SqlHelpers.GetDefaultPrimaryKeyName( Table ), index );
    }

    public SqlPrimaryKeyBuilder SetPrimaryKey(string name, SqlIndexBuilder index)
    {
        Table.ThrowIfRemoved();

        if ( _primaryKey is not null && ReferenceEquals( _primaryKey.Index, index ) )
            return _primaryKey.SetName( name );

        _primaryKey = Table.Schema.Objects.CreatePrimaryKey( Table, name, index, _primaryKey );
        _map[_primaryKey.Name] = _primaryKey;
        AfterCreatePrimaryKey( _primaryKey );
        _primaryKey.Index.AssignPrimaryKey( _primaryKey );
        return _primaryKey;
    }

    public SqlIndexBuilder CreateIndex(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns, bool isUnique = false)
    {
        return CreateIndex( SqlHelpers.GetDefaultIndexName( Table, columns, isUnique ), columns, isUnique );
    }

    public SqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns, bool isUnique = false)
    {
        Table.ThrowIfRemoved();
        var result = Table.Schema.Objects.CreateIndex( Table, name, columns, isUnique );
        _map.Add( result.Name, result );
        AfterCreateIndex( result );
        return result;
    }

    public SqlForeignKeyBuilder CreateForeignKey(SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey( SqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex ), originIndex, referencedIndex );
    }

    public SqlForeignKeyBuilder CreateForeignKey(string name, SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
    {
        Table.ThrowIfRemoved();
        var result = Table.Schema.Objects.CreateForeignKey( Table, name, originIndex, referencedIndex );
        _map.Add( result.Name, result );
        AfterCreateForeignKey( result );
        return result;
    }

    public SqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return CreateCheck( SqlHelpers.GetDefaultCheckName( Table ), condition );
    }

    public SqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        Table.ThrowIfRemoved();
        var result = Table.Schema.Objects.CreateCheck( Table, name, condition );
        _map.Add( result.Name, result );
        AfterCreateCheck( result );
        return result;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var obj ) || ! obj.CanRemove )
            return false;

        obj.Remove();
        Assume.False( _map.ContainsKey( name ) );
        return true;
    }

    [Pure]
    public SqlObjectBuilderEnumerator<SqlConstraintBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlConstraintBuilder>( _map );
    }

    protected virtual void AfterCreateIndex(SqlIndexBuilder index)
    {
        AddCreation( Table, index );
    }

    protected virtual void AfterCreatePrimaryKey(SqlPrimaryKeyBuilder primaryKey)
    {
        AddCreation( Table, primaryKey );
    }

    protected virtual void AfterCreateForeignKey(SqlForeignKeyBuilder foreignKey)
    {
        AddCreation( Table, foreignKey );
    }

    protected virtual void AfterCreateCheck(SqlCheckBuilder check)
    {
        AddCreation( Table, check );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetTable(SqlTableBuilder table)
    {
        Assume.IsNull( _table );
        Assume.Equals( table.Constraints, this );
        Assume.False( table.IsRemoved );
        _table = table;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ChangeName(SqlConstraintBuilder obj, string newName)
    {
        Assume.Equals( obj, _map.GetValueOrDefault( obj.Name ) );
        _map.Add( newName, obj );
        _map.Remove( obj.Name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(SqlConstraintBuilder obj)
    {
        _map.Add( obj.Name, obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(SqlConstraintBuilder obj)
    {
        _map.Remove( obj.Name );
        if ( ReferenceEquals( obj, _primaryKey ) )
            _primaryKey = null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _map.Clear();
        _primaryKey = null;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlConstraintBuilder
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw SqlHelpers.CreateObjectCastException( Table.Database, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlConstraintBuilder
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlConstraintBuilder ISqlConstraintBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlConstraintBuilder? ISqlConstraintBuilderCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.GetPrimaryKey()
    {
        return GetPrimaryKey();
    }

    [Pure]
    ISqlPrimaryKeyBuilder? ISqlConstraintBuilderCollection.TryGetPrimaryKey()
    {
        return TryGetPrimaryKey();
    }

    [Pure]
    ISqlIndexBuilder ISqlConstraintBuilderCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndexBuilder? ISqlConstraintBuilderCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKeyBuilder? ISqlConstraintBuilderCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheckBuilder ISqlConstraintBuilderCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheckBuilder? ISqlConstraintBuilderCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.SetPrimaryKey(ISqlIndexBuilder index)
    {
        return SetPrimaryKey( SqlHelpers.CastOrThrow<SqlIndexBuilder>( Table.Database, index ) );
    }

    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.SetPrimaryKey(string name, ISqlIndexBuilder index)
    {
        return SetPrimaryKey( name, SqlHelpers.CastOrThrow<SqlIndexBuilder>( Table.Database, index ) );
    }

    ISqlIndexBuilder ISqlConstraintBuilderCollection.CreateIndex(
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
    {
        return CreateIndex( columns, isUnique );
    }

    ISqlIndexBuilder ISqlConstraintBuilderCollection.CreateIndex(
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
    {
        return CreateIndex( name, columns, isUnique );
    }

    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.CreateForeignKey(
        ISqlIndexBuilder originIndex,
        ISqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey(
            SqlHelpers.CastOrThrow<SqlIndexBuilder>( Table.Database, originIndex ),
            SqlHelpers.CastOrThrow<SqlIndexBuilder>( Table.Database, referencedIndex ) );
    }

    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.CreateForeignKey(
        string name,
        ISqlIndexBuilder originIndex,
        ISqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey(
            name,
            SqlHelpers.CastOrThrow<SqlIndexBuilder>( Table.Database, originIndex ),
            SqlHelpers.CastOrThrow<SqlIndexBuilder>( Table.Database, referencedIndex ) );
    }

    ISqlCheckBuilder ISqlConstraintBuilderCollection.CreateCheck(SqlConditionNode condition)
    {
        return CreateCheck( condition );
    }

    ISqlCheckBuilder ISqlConstraintBuilderCollection.CreateCheck(string name, SqlConditionNode condition)
    {
        return CreateCheck( name, condition );
    }

    [Pure]
    IEnumerator<ISqlConstraintBuilder> IEnumerable<ISqlConstraintBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
