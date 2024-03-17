using System;
using LfrlAnvil.MySql.Objects;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.MySql;

public sealed class MySqlDatabase : SqlDatabase
{
    internal MySqlDatabase(
        MySqlDatabaseBuilder builder,
        MySqlDatabaseConnector connector,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base( builder, new MySqlSchemaCollection( builder.Schemas ), connector, version, versionRecordsQuery )
    {
        connector.SetDatabase( this );
    }

    public new MySqlSchemaCollection Schemas => ReinterpretCast.To<MySqlSchemaCollection>( base.Schemas );
    public new MySqlDataTypeProvider DataTypes => ReinterpretCast.To<MySqlDataTypeProvider>( base.DataTypes );

    public new MySqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<MySqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new MySqlNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<MySqlNodeInterpreterFactory>( base.NodeInterpreters );
    public new MySqlQueryReaderFactory QueryReaders => ReinterpretCast.To<MySqlQueryReaderFactory>( base.QueryReaders );
    public new MySqlParameterBinderFactory ParameterBinders => ReinterpretCast.To<MySqlParameterBinderFactory>( base.ParameterBinders );
    public new MySqlDatabaseConnector Connector => ReinterpretCast.To<MySqlDatabaseConnector>( base.Connector );
}
