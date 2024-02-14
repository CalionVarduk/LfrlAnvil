using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlColumnBuilderCollection : ISqlColumnBuilderCollection
{
    private readonly Dictionary<string, MySqlColumnBuilder> _map;

    internal MySqlColumnBuilderCollection(MySqlTableBuilder table)
    {
        Table = table;
        DefaultTypeDefinition = table.Database.TypeDefinitions.GetByType( typeof( object ) );
        _map = new Dictionary<string, MySqlColumnBuilder>( StringComparer.OrdinalIgnoreCase );
    }

    public MySqlTableBuilder Table { get; }
    public MySqlColumnTypeDefinition DefaultTypeDefinition { get; private set; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlColumnBuilderCollection.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilderCollection.DefaultTypeDefinition => DefaultTypeDefinition;

    public MySqlColumnBuilderCollection SetDefaultTypeDefinition(MySqlColumnTypeDefinition definition)
    {
        if ( ! ReferenceEquals( Table.Database.TypeDefinitions.TryGetByType( definition.RuntimeType ), definition ) )
            throw new MySqlObjectBuilderException( ExceptionResources.UnrecognizedTypeDefinition( definition ) );

        DefaultTypeDefinition = definition;
        return this;
    }

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlColumnBuilder Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlColumnBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    public MySqlColumnBuilder Create(string name)
    {
        Table.EnsureNotRemoved();
        MySqlHelpers.AssertName( name );

        ref var column = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( column, name ) );

        column = CreateNewColumn( name );
        return column;
    }

    public MySqlColumnBuilder GetOrCreate(string name)
    {
        Table.EnsureNotRemoved();
        MySqlHelpers.AssertName( name );

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
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlColumnBuilder>
    {
        private Dictionary<string, MySqlColumnBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlColumnBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlColumnBuilder Current => _enumerator.Current;
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
    private MySqlColumnBuilder CreateNewColumn(string name)
    {
        var result = new MySqlColumnBuilder( Table, name, DefaultTypeDefinition );
        Table.Database.Changes.ObjectCreated( Table, result );
        return result;
    }

    internal void ChangeName(MySqlColumnBuilder column, string name)
    {
        ref var columnRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( columnRef, name ) );

        columnRef = column;
        _map.Remove( column.Name );
    }

    internal void ClearInto(RentedMemorySequenceSpan<MySqlObjectBuilder> buffer)
    {
        _map.Values.CopyTo( buffer );
        _map.Clear();
    }

    internal void MarkAllAsRemoved()
    {
        foreach ( var column in _map.Values )
            column.MarkAsRemoved();

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
        return SetDefaultTypeDefinition( MySqlHelpers.CastOrThrow<MySqlColumnTypeDefinition>( definition ) );
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
