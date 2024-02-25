using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseMock : SqlDatabase
{
    public SqlDatabaseMock(
        SqlDatabaseBuilderMock builder,
        DbConnectionStringBuilder? connectionString = null,
        Version? version = null,
        Func<IEnumerable<SqlDatabaseVersionRecord>>? versionRecordsProvider = null)
        : base(
            builder,
            new SqlSchemaCollectionMock( builder.Schemas ),
            version ?? new Version( "0.0.0" ),
            new SqlQueryReader<SqlDatabaseVersionRecord>(
                    builder.Dialect,
                    (_, _) => versionRecordsProvider is null
                        ? SqlQueryReaderResult<SqlDatabaseVersionRecord>.Empty
                        : new SqlQueryReaderResult<SqlDatabaseVersionRecord>( null, versionRecordsProvider().ToList() ) )
                .Bind( string.Empty ) )
    {
        ConnectionString = connectionString;
    }

    public DbConnectionStringBuilder? ConnectionString { get; }
    public new SqlSchemaCollectionMock Schemas => ReinterpretCast.To<SqlSchemaCollectionMock>( base.Schemas );

    [Pure]
    public override DbConnection Connect()
    {
        var result = CreateConnection();
        result.Open();
        return result;
    }

    [Pure]
    public override async ValueTask<DbConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var result = CreateConnection();
        await result.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return result;
    }

    [Pure]
    public static SqlDatabaseMock Create(SqlDatabaseBuilderMock builder)
    {
        return new SqlDatabaseMock( builder );
    }

    [Pure]
    private DbConnection CreateConnection()
    {
        return new DbConnectionMock( ServerVersion ) { ConnectionString = ConnectionString?.ToString() ?? string.Empty };
    }
}
