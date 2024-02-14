using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlSchemaCollection : ISqlSchemaCollection
{
    private readonly Dictionary<string, SqlSchema> _map;
    private SqlDatabase? _database;
    private SqlSchema? _default;

    protected SqlSchemaCollection(SqlSchemaBuilderCollection source)
    {
        _map = new Dictionary<string, SqlSchema>( capacity: source.Count, comparer: SqlHelpers.NameComparer );
        _database = null;
        _default = null;
    }

    public int Count => _map.Count;

    public SqlDatabase Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    public SqlSchema Default
    {
        get
        {
            Assume.IsNotNull( _default );
            return _default;
        }
    }

    ISqlDatabase ISqlSchemaCollection.Database => Database;
    ISqlSchema ISqlSchemaCollection.Default => Default;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlSchema Get(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqlSchema? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqlObjectEnumerator<SqlSchema> GetEnumerator()
    {
        return new SqlObjectEnumerator<SqlSchema>( _map );
    }

    internal void SetDatabase(SqlDatabase database, SqlSchemaBuilderCollection source)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Schemas, this );
        _database = database;

        using var deferredObjects = source.Database.ObjectPool.GreedyRent();

        foreach ( var b in source )
        {
            var schema = CreateSchema( b );
            schema.Objects.AddImmediateObjects( b.Objects, deferredObjects );
            _map.Add( schema.Name, schema );
        }

        _default = _map[source.Default.Name];
        SqlSchemaBuilder? deferredSchemaBuilder = null;
        SqlSchema? deferredSchema = null;

        deferredObjects.Refresh();
        foreach ( var b in deferredObjects )
        {
            switch ( b.Type )
            {
                case SqlObjectType.ForeignKey:
                {
                    var fk = ReinterpretCast.To<SqlForeignKeyBuilder>( b );
                    if ( ! ReferenceEquals( deferredSchemaBuilder, fk.OriginIndex.Table.Schema ) )
                    {
                        deferredSchemaBuilder = fk.OriginIndex.Table.Schema;
                        deferredSchema = _map[deferredSchemaBuilder.Name];
                    }

                    Assume.IsNotNull( deferredSchema );
                    var referencedSchema = ReferenceEquals( deferredSchemaBuilder, fk.ReferencedIndex.Table.Schema )
                        ? deferredSchema
                        : _map[fk.ReferencedIndex.Table.Schema.Name];

                    deferredSchema.Objects.AddForeignKey( referencedSchema.Objects, fk );
                    break;
                }
                case SqlObjectType.Unknown:
                {
                    var schemaBuilder = GetSchemaFromUnknown( b );
                    if ( ! ReferenceEquals( deferredSchemaBuilder, schemaBuilder ) )
                    {
                        deferredSchemaBuilder = schemaBuilder;
                        deferredSchema = _map[deferredSchemaBuilder.Name];
                    }

                    Assume.IsNotNull( deferredSchema );
                    deferredSchema.Objects.AddUnknownObject( b );
                    break;
                }
            }
        }
    }

    [Pure]
    protected abstract SqlSchema CreateSchema(SqlSchemaBuilder builder);

    [Pure]
    protected virtual SqlSchemaBuilder GetSchemaFromUnknown(SqlObjectBuilder builder)
    {
        throw new NotSupportedException( ExceptionResources.UnknownObjectsAreUnsupported );
    }

    [Pure]
    ISqlSchema ISqlSchemaCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlSchema? ISqlSchemaCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    IEnumerator<ISqlSchema> IEnumerable<ISqlSchema>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
