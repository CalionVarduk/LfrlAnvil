using System;
using System.Data.Common;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteDatabase : SqlDatabase
{
    internal SqliteDatabase(
        SqliteDatabaseBuilder builder,
        ISqlDatabaseConnector<SqliteConnection> connector,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base(
            builder,
            new SqliteSchemaCollection( builder.Schemas ),
            ReinterpretCast.To<ISqlDatabaseConnector<DbConnection>>( connector ),
            version,
            versionRecordsQuery ) { }

    public new SqliteSchemaCollection Schemas => ReinterpretCast.To<SqliteSchemaCollection>( base.Schemas );
    public new SqliteDataTypeProvider DataTypes => ReinterpretCast.To<SqliteDataTypeProvider>( base.DataTypes );

    public new SqliteColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<SqliteColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new SqliteNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<SqliteNodeInterpreterFactory>( base.NodeInterpreters );
    public new SqliteQueryReaderFactory QueryReaders => ReinterpretCast.To<SqliteQueryReaderFactory>( base.QueryReaders );
    public new SqliteParameterBinderFactory ParameterBinders => ReinterpretCast.To<SqliteParameterBinderFactory>( base.ParameterBinders );

    public new ISqlDatabaseConnector<SqliteConnection> Connector =>
        ReinterpretCast.To<ISqlDatabaseConnector<SqliteConnection>>( base.Connector );

    public override void Dispose()
    {
        base.Dispose();
        if ( Connector is SqliteDatabasePermanentConnector permanentConnector )
            permanentConnector.CloseConnection();
    }
}
