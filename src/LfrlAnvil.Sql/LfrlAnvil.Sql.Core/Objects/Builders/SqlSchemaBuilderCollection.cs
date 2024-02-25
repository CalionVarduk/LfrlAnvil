using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlSchemaBuilderCollection : SqlBuilderApi, ISqlSchemaBuilderCollection
{
    private readonly Dictionary<string, SqlSchemaBuilder> _map;
    private SqlDatabaseBuilder? _database;
    private SqlSchemaBuilder? _default;

    protected SqlSchemaBuilderCollection()
    {
        _database = null;
        _default = null;
        _map = new Dictionary<string, SqlSchemaBuilder>( SqlHelpers.NameComparer );
    }

    public SqlDatabaseBuilder Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    public SqlSchemaBuilder Default
    {
        get
        {
            Assume.IsNotNull( _default );
            return _default;
        }
    }

    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlSchemaBuilderCollection.Default => Default;
    ISqlDatabaseBuilder ISqlSchemaBuilderCollection.Database => Database;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlSchemaBuilder Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlSchemaBuilder? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    public SqlSchemaBuilder Create(string name)
    {
        Database.ThrowIfNameIsInvalid( name );

        ref var schema = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.NameIsAlreadyTaken( schema, name ) );

        schema = CreateSchemaBuilder( name );
        AfterCreateSchema( schema );
        return schema;
    }

    public SqlSchemaBuilder GetOrCreate(string name)
    {
        Database.ThrowIfNameIsInvalid( name );

        ref var schema = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            return schema;

        schema = CreateSchemaBuilder( name );
        AfterCreateSchema( schema );
        return schema;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var schema ) || ! schema.CanRemove )
            return false;

        Assume.NotEquals( schema, Default );
        schema.Remove();
        Assume.False( _map.ContainsKey( name ) );
        return true;
    }

    [Pure]
    public SqlObjectBuilderEnumerator<SqlSchemaBuilder> GetEnumerator()
    {
        return new SqlObjectBuilderEnumerator<SqlSchemaBuilder>( _map );
    }

    protected abstract SqlSchemaBuilder CreateSchemaBuilder(string name);

    protected virtual void AfterCreateSchema(SqlSchemaBuilder schema)
    {
        AddCreation( schema, schema );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDatabase(SqlDatabaseBuilder database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Schemas, this );
        _database = database;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDefault(string defaultSchemaName)
    {
        Assume.IsNull( _default );
        Assume.IsEmpty( _map );
        _default = CreateSchemaBuilder( defaultSchemaName );
        _map.Add( _default.Name, _default );
        AfterCreateSchema( _default );
    }

    internal void ChangeName(SqlSchemaBuilder schema, string newName)
    {
        Assume.Equals( schema, _map.GetValueOrDefault( schema.Name ) );
        Database.ThrowIfNameIsInvalid( newName );

        ref var schemaRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, newName, out var exists )!;
        if ( exists )
            throw SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.NameIsAlreadyTaken( schemaRef, newName ) );

        schemaRef = schema;
        Remove( schema );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(SqlSchemaBuilder schema)
    {
        _map.Remove( schema.Name );
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
