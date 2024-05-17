using System;
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlDatabase : SqlDatabase
{
    internal PostgreSqlDatabase(
        PostgreSqlDatabaseBuilder builder,
        PostgreSqlDatabaseConnector connector,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base( builder, new PostgreSqlSchemaCollection( builder.Schemas ), connector, version, versionRecordsQuery )
    {
        connector.SetDatabase( this );
    }

    /// <inheritdoc cref="SqlDatabase.Schemas" />
    public new PostgreSqlSchemaCollection Schemas => ReinterpretCast.To<PostgreSqlSchemaCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabase.DataTypes" />
    public new PostgreSqlDataTypeProvider DataTypes => ReinterpretCast.To<PostgreSqlDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabase.TypeDefinitions" />
    public new PostgreSqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<PostgreSqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabase.NodeInterpreters" />
    public new PostgreSqlNodeInterpreterFactory NodeInterpreters =>
        ReinterpretCast.To<PostgreSqlNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabase.QueryReaders" />
    public new PostgreSqlQueryReaderFactory QueryReaders => ReinterpretCast.To<PostgreSqlQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabase.ParameterBinders" />
    public new PostgreSqlParameterBinderFactory ParameterBinders =>
        ReinterpretCast.To<PostgreSqlParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabase.Connector" />
    public new PostgreSqlDatabaseConnector Connector => ReinterpretCast.To<PostgreSqlDatabaseConnector>( base.Connector );
}
