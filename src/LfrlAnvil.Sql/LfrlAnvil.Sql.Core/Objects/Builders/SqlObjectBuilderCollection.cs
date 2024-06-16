// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlObjectBuilderCollection" />
public abstract class SqlObjectBuilderCollection : SqlBuilderApi, ISqlObjectBuilderCollection
{
    private readonly Dictionary<string, SqlObjectBuilder> _map;
    private SqlSchemaBuilder? _schema;

    /// <summary>
    /// Creates a new empty <see cref="SqlObjectBuilderCollection"/> instance.
    /// </summary>
    protected SqlObjectBuilderCollection()
    {
        _schema = null;
        _map = new Dictionary<string, SqlObjectBuilder>( SqlHelpers.NameComparer );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.Schema" />
    public SqlSchemaBuilder Schema
    {
        get
        {
            Assume.IsNotNull( _schema );
            return _schema;
        }
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlObjectBuilderCollection.Schema => Schema;

    /// <inheritdoc />
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.Get(string)" />
    [Pure]
    public SqlObjectBuilder Get(string name)
    {
        return _map[name];
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGet(string)" />
    [Pure]
    public SqlObjectBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetTable(string)" />
    [Pure]
    public SqlTableBuilder GetTable(string name)
    {
        return GetTypedObject<SqlTableBuilder>( name, SqlObjectType.Table );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGetTable(string)" />
    [Pure]
    public SqlTableBuilder? TryGetTable(string name)
    {
        return TryGetTypedObject<SqlTableBuilder>( name, SqlObjectType.Table );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetIndex(string)" />
    [Pure]
    public SqlIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<SqlIndexBuilder>( name, SqlObjectType.Index );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public SqlIndexBuilder? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqlIndexBuilder>( name, SqlObjectType.Index );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetPrimaryKey(string)" />
    [Pure]
    public SqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public SqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<SqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public SqlForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<SqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public SqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetCheck(string)" />
    [Pure]
    public SqlCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<SqlCheckBuilder>( name, SqlObjectType.Check );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public SqlCheckBuilder? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqlCheckBuilder>( name, SqlObjectType.Check );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetView(string)" />
    [Pure]
    public SqlViewBuilder GetView(string name)
    {
        return GetTypedObject<SqlViewBuilder>( name, SqlObjectType.View );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.TryGetView(string)" />
    [Pure]
    public SqlViewBuilder? TryGetView(string name)
    {
        return TryGetTypedObject<SqlViewBuilder>( name, SqlObjectType.View );
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.CreateTable(string)" />
    public SqlTableBuilder CreateTable(string name)
    {
        Schema.ThrowIfRemoved();
        Schema.Database.ThrowIfNameIsInvalid( SqlObjectType.Table, name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var table = CreateTableBuilder( name );
        Assume.Equals( table.Name, name );
        obj = table;
        AfterCreateTable( table );
        return table;
    }

    /// <inheritdoc cref="ISqlObjectBuilderCollection.GetOrCreateTable(string)" />
    public SqlTableBuilder GetOrCreateTable(string name)
    {
        Schema.ThrowIfRemoved();
        Schema.Database.ThrowIfNameIsInvalid( SqlObjectType.Table, name );

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

    /// <inheritdoc cref="ISqlObjectBuilderCollection.CreateView(string,SqlQueryExpressionNode)" />
    public SqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        Schema.ThrowIfRemoved();
        Schema.Database.ThrowIfNameIsInvalid( SqlObjectType.View, name );

        var validator = CreateViewSourceValidator();
        validator.Visit( source );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, errors );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var view = CreateViewBuilder( name, source, validator.GetReferencedObjects() );
        Assume.Equals( view.Name, name );
        obj = view;
        AfterCreateView( view );
        return view;
    }

    /// <inheritdoc />
    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var obj ) || ! obj.CanRemove )
            return false;

        obj.Remove();
        Assume.False( _map.ContainsKey( name ) );
        return true;
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{T}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{T}"/> instance.</returns>
    [Pure]
    public SqlObjectBuilderEnumerator<SqlObjectBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlObjectBuilder>( _map );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableBuilder"/> instance.
    /// </summary>
    /// <param name="name">Table's name.</param>
    /// <returns>New <see cref="SqlTableBuilder"/> instance</returns>
    protected abstract SqlTableBuilder CreateTableBuilder(string name);

    /// <summary>
    /// Callback invoked just after the <paramref name="table"/> creation has been processed.
    /// </summary>
    /// <param name="table">Created table.</param>
    protected virtual void AfterCreateTable(SqlTableBuilder table)
    {
        AddCreation( table, table );
    }

    /// <summary>
    /// Creates a new <see cref="SqlViewBuilder"/> instance.
    /// </summary>
    /// <param name="name">View's name.</param>
    /// <param name="source">Underlying source query expression that defines this view.</param>
    /// <param name="referencedObjects">
    /// Collection of objects (tables, views and columns) referenced by this view's <see cref="SqlViewBuilder.Source"/>.
    /// </param>
    /// <returns>New <see cref="SqlViewBuilder"/> instance</returns>
    protected abstract SqlViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects);

    /// <summary>
    /// Callback invoked just after the <paramref name="view"/> creation has been processed.
    /// </summary>
    /// <param name="view">Created view.</param>
    protected virtual void AfterCreateView(SqlViewBuilder view)
    {
        AddCreation( view, view );
    }

    /// <summary>
    /// Creates a new <see cref="SqlIndexBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this index is attached to.</param>
    /// <param name="name">Index's name.</param>
    /// <param name="columns">Collection of columns that define this index.</param>
    /// <param name="isUnique">Specifies whether or not this index is unique.</param>
    /// <param name="referencedColumns">Collection of columns referenced by this index's <see cref="SqlIndexBuilder.Columns"/>.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    protected abstract SqlIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns);

    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKeyBuilder"/> instance.
    /// </summary>
    /// <param name="name">Primary key's name.</param>
    /// <param name="index">Underlying index that defines this primary key.</param>
    /// <returns>New <see cref="SqlPrimaryKeyBuilder"/> instance.</returns>
    protected abstract SqlPrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index);

    /// <summary>
    /// Creates a new <see cref="SqlForeignKeyBuilder"/> instance.
    /// </summary>
    /// <param name="name">Foreign key's name.</param>
    /// <param name="originIndex">SQL index that this foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by this foreign key.</param>
    /// <returns>New <see cref="SqlForeignKeyBuilder"/> instance.</returns>
    protected abstract SqlForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex);

    /// <summary>
    /// Creates a new <see cref="SqlCheckBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this check is attached to.</param>
    /// <param name="name">Check's name.</param>
    /// <param name="condition">Underlying condition of this check constraint.</param>
    /// <param name="referencedColumns">Collection of columns referenced by this check constraint.</param>
    /// <returns>New <see cref="SqlCheckBuilder"/> instance.</returns>
    protected abstract SqlCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns);

    /// <summary>
    /// Throws an exception when an index's columns are not valid.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> that the index belongs to.</param>
    /// <param name="columns">Collection of columns that belong to the index.</param>
    /// <param name="isUnique">Specifies whether or not the index is unique.</param>
    /// <exception cref="SqlObjectBuilderException">When index columns are not considered valid.</exception>
    /// <remarks>
    /// See <see cref="SqlHelpers.AssertIndexColumns(SqlTableBuilder,SqlIndexBuilderColumns{SqlColumnBuilder},bool)"/> for more information.
    /// </remarks>
    protected virtual void ThrowIfIndexColumnsAreInvalid(
        SqlTableBuilder table,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique)
    {
        SqlHelpers.AssertIndexColumns( table, columns, isUnique );
    }

    /// <summary>
    /// Throws an exception when a primary key is not valid.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> that the primary key belongs to.</param>
    /// <param name="index"><see cref="SqlIndexBuilder"/> that is the underlying index of the primary key.</param>
    /// <exception cref="SqlObjectBuilderException">When primary key is not considered valid.</exception>
    /// <remarks>See <see cref="SqlHelpers.AssertPrimaryKey(SqlTableBuilder,SqlIndexBuilder)"/> for more information.</remarks>
    protected virtual void ThrowIfPrimaryKeyIsInvalid(SqlTableBuilder table, SqlIndexBuilder index)
    {
        SqlHelpers.AssertPrimaryKey( table, index );
    }

    /// <summary>
    /// Throws an exception when a foreign key is not valid.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> that the foreign key belongs to.</param>
    /// <param name="originIndex"><see cref="SqlIndexBuilder"/> from which the foreign key originates.</param>
    /// <param name="referencedIndex"><see cref="SqlIndexBuilder"/> which the foreign key references.</param>
    /// <exception cref="SqlObjectBuilderException">When foreign key is not considered valid.</exception>
    /// <remarks>
    /// See <see cref="SqlHelpers.AssertForeignKey(SqlTableBuilder,SqlIndexBuilder,SqlIndexBuilder)"/> for more information.
    /// </remarks>
    protected virtual void ThrowIfForeignKeyIsInvalid(SqlTableBuilder table, SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
    {
        SqlHelpers.AssertForeignKey( table, originIndex, referencedIndex );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableScopeExpressionValidator"/> used for check constraint's
    /// <see cref="SqlCheckBuilder.Condition"/> validation.
    /// </summary>
    /// <returns>New <see cref="SqlTableScopeExpressionValidator"/> instance.</returns>
    [Pure]
    protected virtual SqlTableScopeExpressionValidator CreateCheckConditionValidator(SqlTableBuilder table)
    {
        return new SqlTableScopeExpressionValidator( table );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableScopeExpressionValidator"/> used for index constraint's
    /// <see cref="SqlIndexBuilder.Columns"/> expressions validation.
    /// </summary>
    /// <returns>New <see cref="SqlTableScopeExpressionValidator"/> instance.</returns>
    [Pure]
    protected virtual SqlTableScopeExpressionValidator CreateIndexColumnExpressionValidator(SqlTableBuilder table)
    {
        return new SqlTableScopeExpressionValidator( table );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSchemaScopeExpressionValidator"/> used for view's <see cref="SqlViewBuilder.Source"/> validation.
    /// </summary>
    /// <returns>New <see cref="SqlSchemaScopeExpressionValidator"/> instance.</returns>
    [Pure]
    protected virtual SqlSchemaScopeExpressionValidator CreateViewSourceValidator()
    {
        return new SqlSchemaScopeExpressionValidator( Schema );
    }

    /// <summary>
    /// Checks whether or not an existing object can be replaced with a new primary key.
    /// </summary>
    /// <param name="obj">Existing object to check.</param>
    /// <param name="oldPrimaryKey">Primary key to replace.</param>
    /// <returns>
    /// <b>true</b> when <paramref name="obj"/> is <paramref name="oldPrimaryKey"/>
    /// or is and index with its primary key being <paramref name="oldPrimaryKey"/>, otherwise <b>false</b>.
    /// </returns>
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
        Assume.False( schema.IsRemoved );
        _schema = schema;
    }

    internal SqlIndexBuilder CreateIndex(SqlTableBuilder table, string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique)
    {
        Schema.Database.ThrowIfNameIsInvalid( SqlObjectType.Index, name );

        var validator = CreateIndexColumnExpressionValidator( table );
        foreach ( var orderBy in columns )
            validator.Visit( orderBy.Expression );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( table.Database, errors );

        var indexColumns = new SqlIndexBuilderColumns<SqlColumnBuilder>( columns );
        ThrowIfIndexColumnsAreInvalid( table, indexColumns, isUnique );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = CreateIndexBuilder( table, name, indexColumns, isUnique, validator.GetReferencedColumns() );
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
        table.Database.ThrowIfNameIsInvalid( SqlObjectType.PrimaryKey, name );
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
        table.Database.ThrowIfNameIsInvalid( SqlObjectType.ForeignKey, name );
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
        table.Database.ThrowIfNameIsInvalid( SqlObjectType.Check, name );

        var validator = CreateCheckConditionValidator( table );
        validator.Visit( condition );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( table.Database, errors );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Schema.Database, ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = CreateCheckBuilder( table, name, condition, validator.GetReferencedColumns() );
        obj = result;
        return result;
    }

    internal void ChangeName(SqlObjectBuilder obj, string newName)
    {
        Assume.Equals( obj, _map.GetValueOrDefault( obj.Name ) );
        Schema.Database.ThrowIfNameIsInvalid( obj.Type, newName );

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
