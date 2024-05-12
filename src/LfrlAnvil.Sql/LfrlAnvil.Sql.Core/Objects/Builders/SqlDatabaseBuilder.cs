using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Statements.Compilers;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlDatabaseBuilder" />
public abstract class SqlDatabaseBuilder : SqlBuilderApi, ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;
    private readonly List<Action<SqlDatabaseConnectionChangeEvent>> _connectionChangeCallbacks;

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseBuilder"/> instance.
    /// </summary>
    /// <param name="dialect">Specifies the SQL dialect of this database.</param>
    /// <param name="serverVersion">Current <see cref="DbConnection.ServerVersion"/> of this database.</param>
    /// <param name="defaultSchemaName">Name of the default DB schema.</param>
    /// <param name="dataTypes">Provider of SQL data types.</param>
    /// <param name="typeDefinitions">Provider of column type definitions.</param>
    /// <param name="nodeInterpreters">Factory of node interpreters.</param>
    /// <param name="queryReaders">Factory of query readers.</param>
    /// <param name="parameterBinders">Factory of parameter binders.</param>
    /// <param name="defaultNames">Provider of default SQL object names.</param>
    /// <param name="schemas">Collection of schemas defined in this database.</param>
    /// <param name="changes">Tracker of changes applied to this database.</param>
    protected SqlDatabaseBuilder(
        SqlDialect dialect,
        string serverVersion,
        string defaultSchemaName,
        ISqlDataTypeProvider dataTypes,
        SqlColumnTypeDefinitionProvider typeDefinitions,
        ISqlNodeInterpreterFactory nodeInterpreters,
        SqlQueryReaderFactory queryReaders,
        SqlParameterBinderFactory parameterBinders,
        SqlDefaultObjectNameProvider defaultNames,
        SqlSchemaBuilderCollection schemas,
        SqlDatabaseChangeTracker changes)
    {
        Assume.Equals( queryReaders.Dialect, dialect );
        Assume.Equals( queryReaders.ColumnTypeDefinitions, typeDefinitions );
        Assume.Equals( parameterBinders.Dialect, dialect );
        Assume.Equals( parameterBinders.ColumnTypeDefinitions, typeDefinitions );

        ObjectPool = new MemorySequencePool<SqlObjectBuilder>( minSegmentLength: 32 );
        _connectionChangeCallbacks = new List<Action<SqlDatabaseConnectionChangeEvent>>();
        _idGenerator = new UlongSequenceGenerator();
        DataTypes = dataTypes;
        TypeDefinitions = typeDefinitions;
        NodeInterpreters = nodeInterpreters;
        QueryReaders = queryReaders;
        ParameterBinders = parameterBinders;
        DefaultNames = defaultNames;
        Dialect = dialect;
        ServerVersion = serverVersion;
        UserData = null;
        Changes = changes;
        Changes.SetDatabase( this );
        Schemas = schemas;
        Schemas.SetDatabase( this );
        Schemas.SetDefault( defaultSchemaName );
    }

    /// <inheritdoc />
    public SqlDialect Dialect { get; }

    /// <inheritdoc />
    public string ServerVersion { get; }

    /// <inheritdoc />
    public ISqlDataTypeProvider DataTypes { get; }

    /// <inheritdoc cref="ISqlDatabaseBuilder.TypeDefinitions" />
    public SqlColumnTypeDefinitionProvider TypeDefinitions { get; }

    /// <inheritdoc />
    public ISqlNodeInterpreterFactory NodeInterpreters { get; }

    /// <inheritdoc cref="ISqlDatabaseBuilder.QueryReaders" />
    public SqlQueryReaderFactory QueryReaders { get; }

    /// <inheritdoc cref="ISqlDatabaseBuilder.ParameterBinders" />
    public SqlParameterBinderFactory ParameterBinders { get; }

    /// <inheritdoc cref="ISqlDatabaseBuilder.DefaultNames" />
    public SqlDefaultObjectNameProvider DefaultNames { get; }

    /// <inheritdoc cref="ISqlDatabaseBuilder.Schemas" />
    public SqlSchemaBuilderCollection Schemas { get; }

    /// <inheritdoc cref="ISqlDatabaseBuilder.Changes" />
    public SqlDatabaseChangeTracker Changes { get; }

    /// <summary>
    /// Represents a custom data that can be sent between different DB versions.
    /// </summary>
    public object? UserData { get; set; }

    internal MemorySequencePool<SqlObjectBuilder> ObjectPool { get; }

    internal ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> ConnectionChangeCallbacks =>
        CollectionsMarshal.AsSpan( _connectionChangeCallbacks );

    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlDefaultObjectNameProvider ISqlDatabaseBuilder.DefaultNames => DefaultNames;
    ISqlDatabaseChangeTracker ISqlDatabaseBuilder.Changes => Changes;

    /// <inheritdoc cref="ISqlDatabaseBuilder.AddConnectionChangeCallback(Action{SqlDatabaseConnectionChangeEvent})" />
    public SqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        _connectionChangeCallbacks.Add( callback );
        return this;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="name"/> is valid for a given <paramref name="objectType"/>.
    /// </summary>
    /// <param name="objectType">Object's type.</param>
    /// <param name="name">Object's name to validate.</param>
    /// <returns><b>true</b> when <paramref name="name"/> is valid, otherwise <b>false</b>.</returns>
    [Pure]
    public virtual bool IsValidName(SqlObjectType objectType, string name)
    {
        return ! string.IsNullOrWhiteSpace( name ) && ! name.Contains( SqlHelpers.TextDelimiter );
    }

    /// <summary>
    /// Throws an exception when the provided <paramref name="name"/> is not valid for a given <paramref name="objectType"/>.
    /// </summary>
    /// <param name="objectType">Object's type.</param>
    /// <param name="name">Object's name to validate.</param>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="name"/> is not valid.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfNameIsInvalid(SqlObjectType objectType, string name)
    {
        if ( ! IsValidName( objectType, name ) )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( this, ExceptionResources.InvalidName( name ) ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetNextId()
    {
        return _idGenerator.Generate();
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        return AddConnectionChangeCallback( callback );
    }
}
