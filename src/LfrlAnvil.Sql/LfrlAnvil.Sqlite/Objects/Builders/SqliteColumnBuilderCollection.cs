using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteColumnBuilderCollection : ISqlColumnBuilderCollection
{
    private readonly Dictionary<string, SqliteColumnBuilder> _map;

    internal SqliteColumnBuilderCollection(SqliteTableBuilder table)
    {
        Table = table;
        DefaultTypeDefinition = table.Database.TypeDefinitions.GetDefaultForDataType( SqliteDataType.Any );
        _map = new Dictionary<string, SqliteColumnBuilder>();
    }

    public SqliteTableBuilder Table { get; }
    public SqliteColumnTypeDefinition DefaultTypeDefinition { get; private set; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlColumnBuilderCollection.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilderCollection.DefaultTypeDefinition => DefaultTypeDefinition;

    public SqliteColumnBuilderCollection SetDefaultTypeDefinition(SqliteColumnTypeDefinition definition)
    {
        if ( ! ReferenceEquals( Table.Database.TypeDefinitions.TryGetByType( definition.RuntimeType ), definition ) )
            throw new SqliteObjectBuilderException( ExceptionResources.UnrecognizedTypeDefinition( definition ) );

        DefaultTypeDefinition = definition;
        return this;
    }

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteColumnBuilder Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out SqliteColumnBuilder result)
    {
        return _map.TryGetValue( name, out result );
    }

    public SqliteColumnBuilder Create(string name)
    {
        Table.EnsureNotRemoved();
        SqliteHelpers.AssertName( name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( column, name ) );

        column = CreateNewColumn( name );
        return column;
    }

    public SqliteColumnBuilder GetOrCreate(string name)
    {
        Table.EnsureNotRemoved();
        SqliteHelpers.AssertName( name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            return column;

        column = CreateNewColumn( name );
        return column;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var column ) || ! column.CanRemove )
            return false;

        _map.Remove( name );
        column.Remove();
        return true;
    }

    [Pure]
    public IReadOnlyCollection<SqliteColumnBuilder> AsCollection()
    {
        return _map.Values;
    }

    [Pure]
    public IEnumerator<SqliteColumnBuilder> GetEnumerator()
    {
        return AsCollection().GetEnumerator();
    }

    [Pure]
    private SqliteColumnBuilder CreateNewColumn(string name)
    {
        var result = new SqliteColumnBuilder( Table, name, DefaultTypeDefinition );
        Table.Database.ChangeTracker.ObjectCreated( Table, result );
        return result;
    }

    internal void ChangeName(SqliteColumnBuilder column, string name)
    {
        ref var columnRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( columnRef, name ) );

        columnRef = column;
        _map.Remove( column.Name );
    }

    internal void ClearInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _map.Values.CopyTo( buffer );
        _map.Clear();
    }

    [Pure]
    ISqlColumnBuilder ISqlColumnBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    bool ISqlColumnBuilderCollection.TryGet(string name, [MaybeNullWhen( false )] out ISqlColumnBuilder result)
    {
        if ( TryGet( name, out var column ) )
        {
            result = column;
            return true;
        }

        result = null;
        return false;
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
        return SetDefaultTypeDefinition( SqliteHelpers.CastOrThrow<SqliteColumnTypeDefinition>( definition ) );
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
