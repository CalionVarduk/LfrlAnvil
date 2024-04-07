using System;
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.PostgreSql;

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

    public new PostgreSqlSchemaCollection Schemas => ReinterpretCast.To<PostgreSqlSchemaCollection>( base.Schemas );
    public new PostgreSqlDataTypeProvider DataTypes => ReinterpretCast.To<PostgreSqlDataTypeProvider>( base.DataTypes );

    public new PostgreSqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<PostgreSqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new PostgreSqlNodeInterpreterFactory NodeInterpreters =>
        ReinterpretCast.To<PostgreSqlNodeInterpreterFactory>( base.NodeInterpreters );

    public new PostgreSqlQueryReaderFactory QueryReaders => ReinterpretCast.To<PostgreSqlQueryReaderFactory>( base.QueryReaders );

    public new PostgreSqlParameterBinderFactory ParameterBinders =>
        ReinterpretCast.To<PostgreSqlParameterBinderFactory>( base.ParameterBinders );

    public new PostgreSqlDatabaseConnector Connector => ReinterpretCast.To<PostgreSqlDatabaseConnector>( base.Connector );
}
