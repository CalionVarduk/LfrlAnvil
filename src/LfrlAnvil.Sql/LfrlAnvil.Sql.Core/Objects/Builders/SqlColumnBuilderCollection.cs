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
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlColumnBuilderCollection" />
public abstract class SqlColumnBuilderCollection : SqlBuilderApi, ISqlColumnBuilderCollection
{
    private readonly Dictionary<string, SqlColumnBuilder> _map;
    private SqlTableBuilder? _table;

    /// <summary>
    /// Creates a new empty <see cref="SqlColumnBuilderCollection"/> instance.
    /// </summary>
    /// <param name="defaultTypeDefinition">Specifies the default <see cref="SqlColumnTypeDefinition"/> of newly created columns.</param>
    protected SqlColumnBuilderCollection(SqlColumnTypeDefinition defaultTypeDefinition)
    {
        _table = null;
        _map = new Dictionary<string, SqlColumnBuilder>( SqlHelpers.NameComparer );
        DefaultTypeDefinition = defaultTypeDefinition;
    }

    /// <inheritdoc cref="ISqlColumnBuilderCollection.Table" />
    public SqlTableBuilder Table
    {
        get
        {
            Assume.IsNotNull( _table );
            return _table;
        }
    }

    /// <inheritdoc cref="ISqlColumnBuilderCollection.DefaultTypeDefinition" />
    public SqlColumnTypeDefinition DefaultTypeDefinition { get; private set; }

    /// <inheritdoc />
    public int Count => _map.Count;

    ISqlTableBuilder ISqlColumnBuilderCollection.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilderCollection.DefaultTypeDefinition => DefaultTypeDefinition;

    /// <inheritdoc cref="ISqlColumnBuilderCollection.SetDefaultTypeDefinition(ISqlColumnTypeDefinition)" />
    public SqlColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        Table.ThrowIfRemoved();
        if ( ! Table.Database.TypeDefinitions.Contains( definition ) )
            throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.UnrecognizedTypeDefinition( definition ) );

        DefaultTypeDefinition = definition;
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <inheritdoc cref="ISqlColumnBuilderCollection.Get(string)" />
    [Pure]
    public SqlColumnBuilder Get(string name)
    {
        return _map[name];
    }

    /// <inheritdoc cref="ISqlColumnBuilderCollection.TryGet(string)" />
    [Pure]
    public SqlColumnBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    /// <inheritdoc cref="ISqlColumnBuilderCollection.Create(string)" />
    public SqlColumnBuilder Create(string name)
    {
        Table.ThrowIfRemoved();
        Table.Database.ThrowIfNameIsInvalid( SqlObjectType.Column, name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.NameIsAlreadyTaken( column, name ) );

        column = CreateColumnBuilder( name );
        AfterCreateColumn( column );
        return column;
    }

    /// <inheritdoc cref="ISqlColumnBuilderCollection.GetOrCreate(string)" />
    public SqlColumnBuilder GetOrCreate(string name)
    {
        Table.ThrowIfRemoved();
        Table.Database.ThrowIfNameIsInvalid( SqlObjectType.Column, name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            return column;

        column = CreateColumnBuilder( name );
        AfterCreateColumn( column );
        return column;
    }

    /// <inheritdoc />
    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var column ) || ! column.CanRemove )
            return false;

        column.Remove();
        Assume.False( _map.ContainsKey( name ) );
        return true;
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{T}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{T}"/> instance.</returns>
    [Pure]
    public SqlObjectBuilderEnumerator<SqlColumnBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlColumnBuilder>( _map );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnBuilder"/> instance.
    /// </summary>
    /// <param name="name">Column's name.</param>
    /// <returns>New <see cref="SqlColumnBuilder"/> instance.</returns>
    protected abstract SqlColumnBuilder CreateColumnBuilder(string name);

    /// <summary>
    /// Callback invoked just after the <paramref name="column"/> creation has been processed.
    /// </summary>
    /// <param name="column">Created column.</param>
    protected virtual void AfterCreateColumn(SqlColumnBuilder column)
    {
        AddCreation( Table, column );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetTable(SqlTableBuilder table)
    {
        Assume.IsNull( _table );
        Assume.Equals( table.Columns, this );
        Assume.False( table.IsRemoved );
        Assume.True( table.Database.TypeDefinitions.Contains( DefaultTypeDefinition ) );
        _table = table;
    }

    internal void ChangeName(SqlColumnBuilder column, string newName)
    {
        Assume.Equals( column, _map.GetValueOrDefault( column.Name ) );
        Table.Database.ThrowIfNameIsInvalid( column.Type, newName );

        ref var columnRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, newName, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.NameIsAlreadyTaken( columnRef, newName ) );

        columnRef = column;
        Remove( column );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(SqlColumnBuilder column)
    {
        _map.Remove( column.Name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _map.Clear();
    }

    [Pure]
    ISqlColumnBuilder ISqlColumnBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlColumnBuilder? ISqlColumnBuilderCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    ISqlColumnBuilder ISqlColumnBuilderCollection.Create(string name)
    {
        return Create( name );
    }

    ISqlColumnBuilder ISqlColumnBuilderCollection.GetOrCreate(string name)
    {
        return GetOrCreate( name );
    }

    ISqlColumnBuilderCollection ISqlColumnBuilderCollection.SetDefaultTypeDefinition(ISqlColumnTypeDefinition definition)
    {
        return SetDefaultTypeDefinition( SqlHelpers.CastOrThrow<SqlColumnTypeDefinition>( Table.Database, definition ) );
    }

    [Pure]
    IEnumerator<ISqlColumnBuilder> IEnumerable<ISqlColumnBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
