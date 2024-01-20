using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlDatabaseBuilder : ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;

    internal MySqlDatabaseBuilder(string serverVersion, string commonSchemaName)
    {
        ServerVersion = serverVersion;
        CommonSchemaName = commonSchemaName;
        _idGenerator = new UlongSequenceGenerator();
        DataTypes = new MySqlDataTypeProvider();
        TypeDefinitions = new MySqlColumnTypeDefinitionProvider( DataTypes );
        NodeInterpreters = new MySqlNodeInterpreterFactory( TypeDefinitions, CommonSchemaName );
        QueryReaders = new MySqlQueryReaderFactory( TypeDefinitions );
        ParameterBinders = new MySqlParameterBinderFactory( TypeDefinitions );
        Schemas = new MySqlSchemaBuilderCollection( this, CommonSchemaName );
        ChangeTracker = new MySqlDatabaseChangeTracker( this );
        ObjectPool = new MemorySequencePool<MySqlObjectBuilder>( minSegmentLength: 32 );
        ConnectionChanges = SqlDatabaseConnectionChangeCallbacks.Create();
    }

    public MySqlDataTypeProvider DataTypes { get; }
    public MySqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    public MySqlNodeInterpreterFactory NodeInterpreters { get; private set; }
    public MySqlQueryReaderFactory QueryReaders { get; }
    public MySqlParameterBinderFactory ParameterBinders { get; }
    public MySqlSchemaBuilderCollection Schemas { get; }
    public string ServerVersion { get; }
    public string CommonSchemaName { get; }
    public SqlDialect Dialect => MySqlDialect.Instance;
    public SqlDatabaseCreateMode Mode => ChangeTracker.Mode;
    public bool IsAttached => ChangeTracker.IsAttached;
    internal MySqlDatabaseChangeTracker ChangeTracker { get; }
    internal MemorySequencePool<MySqlObjectBuilder> ObjectPool { get; }
    internal SqlDatabaseConnectionChangeCallbacks ConnectionChanges { get; private set; }
    ISqlDataTypeProvider ISqlDatabaseBuilder.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabaseBuilder.NodeInterpreters => NodeInterpreters;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;

    public ReadOnlySpan<SqlDatabaseBuilderStatement> GetPendingStatements()
    {
        return ChangeTracker.GetPendingStatements();
    }

    public void AddStatement(ISqlStatementNode statement)
    {
        var context = NodeInterpreters.Create().Interpret( statement.Node );
        if ( context.Parameters.Count > 0 )
            throw new MySqlObjectBuilderException( Chain.Create( ExceptionResources.StatementIsParameterized( statement, context ) ) );

        ChangeTracker.AddStatement( SqlDatabaseBuilderStatement.Create( context ) );
    }

    public void AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options = null)
    {
        var context = NodeInterpreters.Create().Interpret( statement.Node );
        var opt = options ?? SqlParameterBinderCreationOptions.Default;
        var executor = ParameterBinders.Create( opt.SetContext( context ) ).Bind( parameters );
        ChangeTracker.AddStatement( SqlDatabaseBuilderStatement.Create( context, executor ) );
    }

    public void AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        var context = NodeInterpreters.Create().Interpret( statement.Node );
        var opt = options ?? SqlParameterBinderCreationOptions.Default;
        var executor = ParameterBinders.Create<TSource>( opt.SetContext( context ) ).Bind( parameters );
        ChangeTracker.AddStatement( SqlDatabaseBuilderStatement.Create( context, executor ) );
    }

    public MySqlDatabaseBuilder SetNodeInterpreterFactory(MySqlNodeInterpreterFactory factory)
    {
        NodeInterpreters = factory;
        return this;
    }

    public MySqlDatabaseBuilder SetAttachedMode(bool enabled = true)
    {
        ChangeTracker.SetAttachedMode( enabled );
        return this;
    }

    public MySqlDatabaseBuilder SetDetachedMode(bool enabled = true)
    {
        ChangeTracker.SetAttachedMode( ! enabled );
        return this;
    }

    public MySqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        ConnectionChanges.AddCallback( callback );
        return this;
    }

    [Pure]
    internal ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> GetPendingConnectionChangeCallbacks()
    {
        var result = ConnectionChanges.GetPendingCallbacks();
        ConnectionChanges = ConnectionChanges.UpdateFirstPendingCallbackIndex();
        return result;
    }

    internal ulong GetNextId()
    {
        return _idGenerator.Generate();
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.SetNodeInterpreterFactory(ISqlNodeInterpreterFactory factory)
    {
        return SetNodeInterpreterFactory( MySqlHelpers.CastOrThrow<MySqlNodeInterpreterFactory>( factory ) );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.SetDetachedMode(bool enabled)
    {
        return SetDetachedMode( enabled );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.SetAttachedMode(bool enabled)
    {
        return SetAttachedMode( enabled );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        return AddConnectionChangeCallback( callback );
    }
}
