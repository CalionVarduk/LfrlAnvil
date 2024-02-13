using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
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
        Changes = new MySqlDatabaseChangeTracker( this );
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
    public SqlDatabaseCreateMode Mode => Changes.Mode;
    public bool IsAttached => Changes.IsAttached;
    public MySqlDatabaseChangeTracker Changes { get; }
    internal MemorySequencePool<MySqlObjectBuilder> ObjectPool { get; }
    internal SqlDatabaseConnectionChangeCallbacks ConnectionChanges { get; private set; }
    ISqlDataTypeProvider ISqlDatabaseBuilder.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabaseBuilder.NodeInterpreters => NodeInterpreters;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;
    ISqlDatabaseChangeTracker ISqlDatabaseBuilder.Changes => Changes;

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

    ISqlDatabaseBuilder ISqlDatabaseBuilder.AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        return AddConnectionChangeCallback( callback );
    }
}
