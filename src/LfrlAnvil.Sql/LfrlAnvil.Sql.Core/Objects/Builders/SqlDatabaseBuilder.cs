using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Statements.Compilers;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

// TODO:
// THEN, create core db objects (not builders)
// THEN, create type definition provider builder
// THEN, refactor db builder further, with dedicated configuration object (allows to override default providers)
// ^ it may actually be enough to define a struct, since actual configuration has to be exposed for concrete db builders
// THEN, create core db factory (this could also use nsubstitute mocks replacement) (also, db builder command action may need some rethinking)
// ^ or at least create an internal loggable IDbCommand (could be publicly available, with a before+after callbacks + time tracking)
// ^ if made public, then could use an async version as well
// ^ also, I keep forgetting, ExecuteScalar wrapper for query/multi readers
// THEN, create better core node interpreter
// THEN, create db version object that works with core classes rather than interfaces
// THEN, core might actually be done?
// THEN, update sqlite to work fully with new core
// THEN, update mysql to work fully with new core
// THEN, change IXs so that they accept an array of SqlOrderByNode
// ^ including extensions that still allow to provide 'bare' indexed columns
// THEN (?), add possibility to registered generated/computed columns (low priority)

public abstract class SqlDatabaseBuilder : SqlBuilderApi, ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;

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
        ConnectionChanges = SqlDatabaseConnectionChangeCallbacks.Create();
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
        // TODO: SetDefault may have to be called later, when DB ctor/init is fully done
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
    internal SqlDatabaseConnectionChangeCallbacks ConnectionChanges { get; private set; }

    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlDatabaseChangeTracker ISqlDatabaseBuilder.Changes => Changes;

    public SqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        ConnectionChanges.AddCallback( callback );
        return this;
    }

    [Pure]
    public virtual bool IsValidName(string name)
    {
        return ! string.IsNullOrWhiteSpace( name ) && ! name.Contains( '\'' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfNameIsInvalid(string name)
    {
        if ( ! IsValidName( name ) )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( this, ExceptionResources.InvalidName( name ) ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetNextId()
    {
        return _idGenerator.Generate();
    }

    [Pure]
    internal ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> GetPendingConnectionChangeCallbacks()
    {
        var result = ConnectionChanges.GetPendingCallbacks();
        ConnectionChanges = ConnectionChanges.UpdateFirstPendingCallbackIndex();
        return result;
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        return AddConnectionChangeCallback( callback );
    }
}
