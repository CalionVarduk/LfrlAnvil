using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteSchemaBuilderCollection : ISqlSchemaBuilderCollection
{
    private readonly Dictionary<string, SqliteSchemaBuilder> _map;

    internal SqliteSchemaBuilderCollection(SqliteDatabaseBuilder database, string defaultSchemaName)
    {
        Database = database;
        AssertName( defaultSchemaName );
        Default = new SqliteSchemaBuilder( Database, defaultSchemaName );
        _map = new Dictionary<string, SqliteSchemaBuilder>( StringComparer.OrdinalIgnoreCase ) { { Default.Name, Default } };
    }

    public SqliteDatabaseBuilder Database { get; }
    public SqliteSchemaBuilder Default { get; }
    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlSchemaBuilderCollection.Default => Default;
    ISqlDatabaseBuilder ISqlSchemaBuilderCollection.Database => Database;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteSchemaBuilder GetSchema(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqliteSchemaBuilder? TryGetSchema(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    public SqliteSchemaBuilder Create(string name)
    {
        AssertName( name );
        ref var schema = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( schema, name ) );

        schema = CreateNewSchema( name );
        return schema;
    }

    public SqliteSchemaBuilder GetOrCreate(string name)
    {
        AssertName( name );
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

    public struct Enumerator : IEnumerator<SqliteSchemaBuilder>
    {
        private Dictionary<string, SqliteSchemaBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteSchemaBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteSchemaBuilder Current => _enumerator.Current;
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

    internal void ChangeName(SqliteSchemaBuilder schema, string name)
    {
        ref var schemaRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( schemaRef, name ) );

        schemaRef = schema;
        _map.Remove( schema.Name );
    }

    [Pure]
    private SqliteSchemaBuilder CreateNewSchema(string name)
    {
        return new SqliteSchemaBuilder( Database, name );
    }

    private static void AssertName(string name)
    {
        if ( name.Length > 0 )
            SqliteHelpers.AssertName( name );
    }

    [Pure]
    ISqlSchemaBuilder ISqlSchemaBuilderCollection.GetSchema(string name)
    {
        return GetSchema( name );
    }

    [Pure]
    ISqlSchemaBuilder? ISqlSchemaBuilderCollection.TryGetSchema(string name)
    {
        return TryGetSchema( name );
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
