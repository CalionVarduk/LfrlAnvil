using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Statements.Compilers;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlDatabaseBuilder : SqlBuilderApi, ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;
    private readonly List<Action<SqlDatabaseConnectionChangeEvent>> _connectionChangeCallbacks;

    protected SqlDatabaseBuilder(
        SqlDialect dialect,
        string serverVersion,
        string defaultSchemaName,
        ISqlDataTypeProvider dataTypes,
        SqlColumnTypeDefinitionProvider typeDefinitions,
        ISqlNodeInterpreterFactory nodeInterpreters,
        SqlQueryReaderFactory queryReaders,
        SqlParameterBinderFactory parameterBinders,
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
        Dialect = dialect;
        ServerVersion = serverVersion;
        Changes = changes;
        Changes.SetDatabase( this );
        Schemas = schemas;
        Schemas.SetDatabase( this );
        Schemas.SetDefault( defaultSchemaName );
    }

    public SqlDialect Dialect { get; }
    public string ServerVersion { get; }
    public ISqlDataTypeProvider DataTypes { get; }
    public SqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    public ISqlNodeInterpreterFactory NodeInterpreters { get; }
    public SqlQueryReaderFactory QueryReaders { get; }
    public SqlParameterBinderFactory ParameterBinders { get; }
    public SqlSchemaBuilderCollection Schemas { get; }
    public SqlDatabaseChangeTracker Changes { get; }
    internal MemorySequencePool<SqlObjectBuilder> ObjectPool { get; }

    internal ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> ConnectionChangeCallbacks =>
        CollectionsMarshal.AsSpan( _connectionChangeCallbacks );

    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlDatabaseChangeTracker ISqlDatabaseBuilder.Changes => Changes;

    public SqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        _connectionChangeCallbacks.Add( callback );
        return this;
    }

    [Pure]
    public virtual bool IsValidName(SqlObjectType objectType, string name)
    {
        return ! string.IsNullOrWhiteSpace( name ) && ! name.Contains( SqlHelpers.TextDelimiter );
    }

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
