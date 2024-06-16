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
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlConstraintBuilderCollection" />
public abstract class SqlConstraintBuilderCollection : SqlBuilderApi, ISqlConstraintBuilderCollection
{
    private readonly Dictionary<string, SqlConstraintBuilder> _map;
    private SqlTableBuilder? _table;
    private SqlPrimaryKeyBuilder? _primaryKey;

    /// <summary>
    /// Creates a new empty <see cref="SqlConstraintBuilderCollection"/> instance.
    /// </summary>
    protected SqlConstraintBuilderCollection()
    {
        _table = null;
        _primaryKey = null;
        _map = new Dictionary<string, SqlConstraintBuilder>( SqlHelpers.NameComparer );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.Table" />
    public SqlTableBuilder Table
    {
        get
        {
            Assume.IsNotNull( _table );
            return _table;
        }
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    ISqlTableBuilder ISqlConstraintBuilderCollection.Table => Table;

    /// <inheritdoc />
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.Get(string)" />
    [Pure]
    public SqlConstraintBuilder Get(string name)
    {
        return _map[name];
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.TryGet(string)" />
    [Pure]
    public SqlConstraintBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.GetPrimaryKey()" />
    [Pure]
    public SqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return TryGetPrimaryKey()
            ?? throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.PrimaryKeyIsMissing( Table ) );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.TryGetPrimaryKey()" />
    [Pure]
    public SqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return _primaryKey;
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.GetIndex(string)" />
    [Pure]
    public SqlIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<SqlIndexBuilder>( name, SqlObjectType.Index );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public SqlIndexBuilder? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqlIndexBuilder>( name, SqlObjectType.Index );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public SqlForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<SqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public SqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.GetCheck(string)" />
    [Pure]
    public SqlCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<SqlCheckBuilder>( name, SqlObjectType.Check );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public SqlCheckBuilder? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqlCheckBuilder>( name, SqlObjectType.Check );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.SetPrimaryKey(ISqlIndexBuilder)" />
    public SqlPrimaryKeyBuilder SetPrimaryKey(SqlIndexBuilder index)
    {
        return SetPrimaryKey( SqlHelpers.GetDefaultPrimaryKeyName( Table ), index );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.SetPrimaryKey(string,ISqlIndexBuilder)" />
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

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.CreateIndex(ReadOnlyArray{SqlOrderByNode},bool)" />
    public SqlIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return CreateIndex(
            SqlHelpers.GetDefaultIndexName( Table, new SqlIndexBuilderColumns<ISqlColumnBuilder>( columns ), isUnique ),
            columns,
            isUnique );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.CreateIndex(string,ReadOnlyArray{SqlOrderByNode},bool)" />
    public SqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        Table.ThrowIfRemoved();
        var result = Table.Schema.Objects.CreateIndex( Table, name, columns, isUnique );
        _map.Add( result.Name, result );
        AfterCreateIndex( result );
        return result;
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.CreateForeignKey(ISqlIndexBuilder,ISqlIndexBuilder)" />
    public SqlForeignKeyBuilder CreateForeignKey(SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey( SqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex ), originIndex, referencedIndex );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.CreateForeignKey(string,ISqlIndexBuilder,ISqlIndexBuilder)" />
    public SqlForeignKeyBuilder CreateForeignKey(string name, SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
    {
        Table.ThrowIfRemoved();
        var result = Table.Schema.Objects.CreateForeignKey( Table, name, originIndex, referencedIndex );
        _map.Add( result.Name, result );
        AfterCreateForeignKey( result );
        return result;
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.CreateCheck(SqlConditionNode)" />
    public SqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return CreateCheck( SqlHelpers.GetDefaultCheckName( Table ), condition );
    }

    /// <inheritdoc cref="ISqlConstraintBuilderCollection.CreateCheck(string,SqlConditionNode)" />
    public SqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        Table.ThrowIfRemoved();
        var result = Table.Schema.Objects.CreateCheck( Table, name, condition );
        _map.Add( result.Name, result );
        AfterCreateCheck( result );
        return result;
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
    public SqlObjectBuilderEnumerator<SqlConstraintBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlConstraintBuilder>( _map );
    }

    /// <summary>
    /// Callback invoked just after the <paramref name="index"/> creation has been processed.
    /// </summary>
    /// <param name="index">Created index.</param>
    protected virtual void AfterCreateIndex(SqlIndexBuilder index)
    {
        AddCreation( Table, index );
    }

    /// <summary>
    /// Callback invoked just after the <paramref name="primaryKey"/> creation has been processed.
    /// </summary>
    /// <param name="primaryKey">Created primary key.</param>
    protected virtual void AfterCreatePrimaryKey(SqlPrimaryKeyBuilder primaryKey)
    {
        AddCreation( Table, primaryKey );
    }

    /// <summary>
    /// Callback invoked just after the <paramref name="foreignKey"/> creation has been processed.
    /// </summary>
    /// <param name="foreignKey">Created foreign key.</param>
    protected virtual void AfterCreateForeignKey(SqlForeignKeyBuilder foreignKey)
    {
        AddCreation( Table, foreignKey );
    }

    /// <summary>
    /// Callback invoked just after the <paramref name="check"/> creation has been processed.
    /// </summary>
    /// <param name="check">Created check.</param>
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

    ISqlIndexBuilder ISqlConstraintBuilderCollection.CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique)
    {
        return CreateIndex( columns, isUnique );
    }

    ISqlIndexBuilder ISqlConstraintBuilderCollection.CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique)
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
