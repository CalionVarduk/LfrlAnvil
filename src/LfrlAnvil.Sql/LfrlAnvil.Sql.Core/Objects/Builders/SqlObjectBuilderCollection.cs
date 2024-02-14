using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlObjectBuilderCollection : SqlBuilderApi, ISqlObjectBuilderCollection
{
    private readonly Dictionary<string, SqlObjectBuilder> _map;
    private SqlSchemaBuilder? _schema;

    protected SqlObjectBuilderCollection()
    {
        _schema = null;
        _map = new Dictionary<string, SqlObjectBuilder>( SqlHelpers.NameComparer );
    }

    public SqlSchemaBuilder Schema
    {
        get
        {
            Assume.IsNotNull( _schema );
            return _schema;
        }
    }

    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlObjectBuilderCollection.Schema => Schema;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlObjectBuilder Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlObjectBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqlTableBuilder GetTable(string name)
    {
        return GetTypedObject<SqlTableBuilder>( name, SqlObjectType.Table );
    }

    [Pure]
    public SqlTableBuilder? TryGetTable(string name)
    {
        return TryGetTypedObject<SqlTableBuilder>( name, SqlObjectType.Table );
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
    public SqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public SqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<SqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
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

    [Pure]
    public SqlViewBuilder GetView(string name)
    {
        return GetTypedObject<SqlViewBuilder>( name, SqlObjectType.View );
    }

    [Pure]
    public SqlViewBuilder? TryGetView(string name)
    {
        return TryGetTypedObject<SqlViewBuilder>( name, SqlObjectType.View );
    }

    public SqlTableBuilder CreateTable(string name)
    {
        Schema.ThrowIfRemoved();
        Schema.Database.ThrowIfNameIsInvalid( name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var table = CreateTableBuilder( name );
        Assume.Equals( table.Name, name );
        obj = table;
        AfterCreateTable( table );
        return table;
    }

    public SqlTableBuilder GetOrCreateTable(string name)
    {
        // TODO:
        // move name validation to configurable db builder interface (low priority, later)
        // maybe include this in the interface responsible for default names
        Schema.ThrowIfRemoved();
        Schema.Database.ThrowIfNameIsInvalid( name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
        {
            if ( obj.Type != SqlObjectType.Table )
                throw SqlHelpers.CreateObjectCastException( Schema.Database, typeof( SqlTableBuilder ), obj.GetType() );

            return ReinterpretCast.To<SqlTableBuilder>( obj );
        }

        var table = CreateTableBuilder( name );
        Assume.Equals( table.Name, name );
        obj = table;
        AfterCreateTable( table );
        return table;
    }

    public SqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        Schema.ThrowIfRemoved();
        Schema.Database.ThrowIfNameIsInvalid( name );

        var visitor = SqlViewBuilder.AssertSourceNode( Schema, source );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var view = CreateViewBuilder( name, source, visitor.GetReferencedObjects() );
        Assume.Equals( view.Name, name );
        obj = view;
        AfterCreateView( view );
        return view;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var obj ) || ! obj.CanRemove )
            return false;

        obj.Remove();
        Assume.Equals( _map.ContainsKey( name ), false );
        return true;
    }

    [Pure]
    public SqlObjectBuilderEnumerator<SqlObjectBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlObjectBuilder>( _map );
    }

    protected abstract SqlTableBuilder CreateTableBuilder(string name);

    protected virtual void AfterCreateTable(SqlTableBuilder table)
    {
        AddCreation( table, table );
    }

    protected abstract SqlViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects);

    protected virtual void AfterCreateView(SqlViewBuilder view)
    {
        AddCreation( view, view );
    }

    protected abstract SqlIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique);

    protected abstract SqlPrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index);

    protected abstract SqlForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex);

    protected abstract SqlCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns);

    protected virtual void ThrowIfIndexColumnsAreInvalid(
        SqlTableBuilder table,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns)
    {
        SqlHelpers.AssertIndexColumns( table, columns );
    }

    protected virtual void ThrowIfPrimaryKeyIsInvalid(SqlTableBuilder table, SqlIndexBuilder index)
    {
        SqlHelpers.AssertPrimaryKey( table, index );
    }

    protected virtual void ThrowIfForeignKeyIsInvalid(SqlTableBuilder table, SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
    {
        SqlHelpers.AssertForeignKey( table, originIndex, referencedIndex );
    }

    [Pure]
    protected static bool CanReplaceWithPrimaryKey(SqlObjectBuilder obj, SqlPrimaryKeyBuilder? oldPrimaryKey)
    {
        if ( oldPrimaryKey is null )
            return false;

        if ( ReferenceEquals( obj, oldPrimaryKey ) )
            return true;

        return obj.Type == SqlObjectType.Index && ReferenceEquals( ReinterpretCast.To<SqlIndexBuilder>( obj ).PrimaryKey, oldPrimaryKey );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetSchema(SqlSchemaBuilder schema)
    {
        Assume.IsNull( _schema );
        Assume.Equals( schema.Objects, this );
        Assume.Equals( schema.IsRemoved, false );
        _schema = schema;
    }

    internal SqlIndexBuilder CreateIndex(
        SqlTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
    {
        Schema.Database.ThrowIfNameIsInvalid( name );
        ThrowIfIndexColumnsAreInvalid( table, columns );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = CreateIndexBuilder( table, name, columns, isUnique );
        Assume.Equals( result.Name, name );
        obj = result;
        return result;
    }

    internal SqlPrimaryKeyBuilder CreatePrimaryKey(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilder index,
        SqlPrimaryKeyBuilder? oldPrimaryKey)
    {
        table.Database.ThrowIfNameIsInvalid( name );
        ThrowIfPrimaryKeyIsInvalid( table, index );

        if ( _map.TryGetValue( name, out var obj ) && ! CanReplaceWithPrimaryKey( obj, oldPrimaryKey ) )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        oldPrimaryKey?.Index.Remove();

        var result = CreatePrimaryKeyBuilder( name, index );
        _map[name] = result;
        return result;
    }

    internal SqlForeignKeyBuilder CreateForeignKey(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        table.Database.ThrowIfNameIsInvalid( name );
        ThrowIfForeignKeyIsInvalid( table, originIndex, referencedIndex );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = CreateForeignKeyBuilder( name, originIndex, referencedIndex );
        obj = result;
        return result;
    }

    internal SqlCheckBuilder CreateCheck(SqlTableBuilder table, string name, SqlConditionNode condition)
    {
        table.Database.ThrowIfNameIsInvalid( name );
        var visitor = SqlCheckBuilder.AssertConditionNode( table, condition );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = CreateCheckBuilder( table, name, condition, visitor.GetReferencedColumns() );
        obj = result;
        return result;
    }

    internal void ChangeName(SqlObjectBuilder obj, string newName)
    {
        Assume.Equals( obj, _map.GetValueOrDefault( obj.Name ) );
        Schema.Database.ThrowIfNameIsInvalid( newName );

        ref var objRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, newName, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, newName ) );

        objRef = obj;
        Remove( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(SqlObjectBuilder obj)
    {
        _map.Add( obj.Name, obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(SqlObjectBuilder obj)
    {
        _map.Remove( obj.Name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _map.Clear();
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlObjectBuilder
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw SqlHelpers.CreateObjectCastException( Schema.Database, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlObjectBuilder
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlObjectBuilder ISqlObjectBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlObjectBuilder? ISqlObjectBuilderCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    ISqlTableBuilder ISqlObjectBuilderCollection.GetTable(string name)
    {
        return GetTable( name );
    }

    [Pure]
    ISqlTableBuilder? ISqlObjectBuilderCollection.TryGetTable(string name)
    {
        return TryGetTable( name );
    }

    [Pure]
    ISqlIndexBuilder ISqlObjectBuilderCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndexBuilder? ISqlObjectBuilderCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlPrimaryKeyBuilder ISqlObjectBuilderCollection.GetPrimaryKey(string name)
    {
        return GetPrimaryKey( name );
    }

    [Pure]
    ISqlPrimaryKeyBuilder? ISqlObjectBuilderCollection.TryGetPrimaryKey(string name)
    {
        return TryGetPrimaryKey( name );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlObjectBuilderCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKeyBuilder? ISqlObjectBuilderCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheckBuilder ISqlObjectBuilderCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheckBuilder? ISqlObjectBuilderCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    [Pure]
    ISqlViewBuilder ISqlObjectBuilderCollection.GetView(string name)
    {
        return GetView( name );
    }

    [Pure]
    ISqlViewBuilder? ISqlObjectBuilderCollection.TryGetView(string name)
    {
        return TryGetView( name );
    }

    ISqlTableBuilder ISqlObjectBuilderCollection.CreateTable(string name)
    {
        return CreateTable( name );
    }

    ISqlTableBuilder ISqlObjectBuilderCollection.GetOrCreateTable(string name)
    {
        return GetOrCreateTable( name );
    }

    ISqlViewBuilder ISqlObjectBuilderCollection.CreateView(string name, SqlQueryExpressionNode source)
    {
        return CreateView( name, source );
    }

    [Pure]
    IEnumerator<ISqlObjectBuilder> IEnumerable<ISqlObjectBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
