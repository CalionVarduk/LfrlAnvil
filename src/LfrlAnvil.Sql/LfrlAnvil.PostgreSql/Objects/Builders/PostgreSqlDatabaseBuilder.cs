using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlDatabaseBuilder : SqlDatabaseBuilder
{
    internal PostgreSqlDatabaseBuilder(
        string serverVersion,
        string defaultSchemaName,
        SqlDefaultObjectNameProvider defaultNames,
        PostgreSqlDataTypeProvider dataTypes,
        PostgreSqlColumnTypeDefinitionProvider typeDefinitions,
        PostgreSqlNodeInterpreterFactory nodeInterpreters,
        SqlOptionalFunctionalityResolution virtualGeneratedColumnStorageResolution)
        : base(
            PostgreSqlDialect.Instance,
            serverVersion,
            defaultSchemaName,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            new PostgreSqlQueryReaderFactory( typeDefinitions ),
            new PostgreSqlParameterBinderFactory( typeDefinitions ),
            defaultNames,
            new PostgreSqlSchemaBuilderCollection(),
            new PostgreSqlDatabaseChangeTracker() )
    {
        Assume.IsDefined( virtualGeneratedColumnStorageResolution );
        VirtualGeneratedColumnStorageResolution = virtualGeneratedColumnStorageResolution;

        if ( ! defaultSchemaName.Equals( PostgreSqlHelpers.DefaultVersionHistoryName.Schema ) )
            Schemas.Create( PostgreSqlHelpers.DefaultVersionHistoryName.Schema );
    }

    public SqlOptionalFunctionalityResolution VirtualGeneratedColumnStorageResolution { get; }

    public new PostgreSqlSchemaBuilderCollection Schemas => ReinterpretCast.To<PostgreSqlSchemaBuilderCollection>( base.Schemas );
    public new PostgreSqlDataTypeProvider DataTypes => ReinterpretCast.To<PostgreSqlDataTypeProvider>( base.DataTypes );

    public new PostgreSqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<PostgreSqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new PostgreSqlNodeInterpreterFactory NodeInterpreters =>
        ReinterpretCast.To<PostgreSqlNodeInterpreterFactory>( base.NodeInterpreters );

    public new PostgreSqlQueryReaderFactory QueryReaders => ReinterpretCast.To<PostgreSqlQueryReaderFactory>( base.QueryReaders );

    public new PostgreSqlParameterBinderFactory ParameterBinders =>
        ReinterpretCast.To<PostgreSqlParameterBinderFactory>( base.ParameterBinders );

    public new PostgreSqlDatabaseChangeTracker Changes => ReinterpretCast.To<PostgreSqlDatabaseChangeTracker>( base.Changes );

    public new PostgreSqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    [Pure]
    public override bool IsValidName(SqlObjectType objectType, string name)
    {
        return base.IsValidName( objectType, name ) && ! name.Contains( '"' );
    }
}
