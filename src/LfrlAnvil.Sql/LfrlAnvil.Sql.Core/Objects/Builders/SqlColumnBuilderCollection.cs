using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlColumnBuilderCollection : SqlBuilderApi, ISqlColumnBuilderCollection
{
    private readonly Dictionary<string, SqlColumnBuilder> _map;
    private SqlTableBuilder? _table;

    protected SqlColumnBuilderCollection(SqlColumnTypeDefinition defaultTypeDefinition)
    {
        _table = null;
        _map = new Dictionary<string, SqlColumnBuilder>( SqlHelpers.NameComparer );
        DefaultTypeDefinition = defaultTypeDefinition;
    }

    public SqlTableBuilder Table
    {
        get
        {
            Assume.IsNotNull( _table );
            return _table;
        }
    }

    public SqlColumnTypeDefinition DefaultTypeDefinition { get; private set; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlColumnBuilderCollection.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilderCollection.DefaultTypeDefinition => DefaultTypeDefinition;

    public SqlColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        Table.ThrowIfRemoved();
        if ( ! Table.Database.TypeDefinitions.Contains( definition ) )
            throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.UnrecognizedTypeDefinition( definition ) );

        DefaultTypeDefinition = definition;
        return this;
    }

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlColumnBuilder Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlColumnBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    public SqlColumnBuilder Create(string name)
    {
        Table.ThrowIfRemoved();
        Table.Database.ThrowIfNameIsInvalid( name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Table.Database, ExceptionResources.NameIsAlreadyTaken( column, name ) );

        column = CreateColumnBuilder( name );
        AfterCreateColumn( column );
        return column;
    }

    public SqlColumnBuilder GetOrCreate(string name)
    {
        Table.ThrowIfRemoved();
        Table.Database.ThrowIfNameIsInvalid( name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            return column;

        column = CreateColumnBuilder( name );
        AfterCreateColumn( column );
        return column;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var column ) || ! column.CanRemove )
            return false;

        column.Remove();
        Assume.Equals( _map.ContainsKey( name ), false );
        return true;
    }

    [Pure]
    public SqlObjectBuilderEnumerator<SqlColumnBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlColumnBuilder>( _map );
    }

    protected abstract SqlColumnBuilder CreateColumnBuilder(string name);

    protected virtual void AfterCreateColumn(SqlColumnBuilder column)
    {
        AddCreation( Table, column );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetTable(SqlTableBuilder table)
    {
        Assume.IsNull( _table );
        Assume.Equals( table.Columns, this );
        Assume.Equals( table.IsRemoved, false );
        Assume.Equals( table.Database.TypeDefinitions.Contains( DefaultTypeDefinition ), true );
        _table = table;
    }

    internal void ChangeName(SqlColumnBuilder column, string newName)
    {
        Assume.Equals( column, _map.GetValueOrDefault( column.Name ) );
        Table.Database.ThrowIfNameIsInvalid( newName );

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
