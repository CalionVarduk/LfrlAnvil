using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlSchemaBuilderCollection : ISqlSchemaBuilderCollection
{
    private readonly Dictionary<string, MySqlSchemaBuilder> _map;

    internal MySqlSchemaBuilderCollection(MySqlDatabaseBuilder database, string defaultSchemaName)
    {
        Database = database;
        MySqlHelpers.AssertName( defaultSchemaName );
        Default = new MySqlSchemaBuilder( Database, defaultSchemaName );
        _map = new Dictionary<string, MySqlSchemaBuilder>( StringComparer.OrdinalIgnoreCase ) { { Default.Name, Default } };
    }

    public MySqlDatabaseBuilder Database { get; }
    public MySqlSchemaBuilder Default { get; }
    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlSchemaBuilderCollection.Default => Default;
    ISqlDatabaseBuilder ISqlSchemaBuilderCollection.Database => Database;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlSchemaBuilder Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlSchemaBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    public MySqlSchemaBuilder Create(string name)
    {
        MySqlHelpers.AssertName( name );
        ref var schema = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( schema, name ) );

        schema = CreateNewSchema( name );
        return schema;
    }

    public MySqlSchemaBuilder GetOrCreate(string name)
    {
        MySqlHelpers.AssertName( name );
        ref var schema = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            return schema;

        schema = CreateNewSchema( name );
        return schema;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var schema ) || ! schema.CanRemove )
            return false;

        _map.Remove( name );
        schema.Remove();
        return true;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlSchemaBuilder>
    {
        private Dictionary<string, MySqlSchemaBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlSchemaBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlSchemaBuilder Current => _enumerator.Current;
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

    internal void ChangeName(MySqlSchemaBuilder schema, string name)
    {
        ref var schemaRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( schemaRef, name ) );

        schemaRef = schema;
        _map.Remove( schema.Name );
    }

    [Pure]
    private MySqlSchemaBuilder CreateNewSchema(string name)
    {
        var result = new MySqlSchemaBuilder( Database, name );
        Database.Changes.SchemaCreated( name );
        return result;
    }

    [Pure]
    ISqlSchemaBuilder ISqlSchemaBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlSchemaBuilder? ISqlSchemaBuilderCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    ISqlSchemaBuilder ISqlSchemaBuilderCollection.Create(string name)
    {
        return Create( name );
    }

    ISqlSchemaBuilder ISqlSchemaBuilderCollection.GetOrCreate(string name)
    {
        return GetOrCreate( name );
    }

    [Pure]
    IEnumerator<ISqlSchemaBuilder> IEnumerable<ISqlSchemaBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
