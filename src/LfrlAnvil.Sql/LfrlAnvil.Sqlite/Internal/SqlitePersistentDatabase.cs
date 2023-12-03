using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePersistentDatabase : SqliteDatabase
{
    [DebuggerBrowsable( DebuggerBrowsableState.Never )]
    private readonly string _connectionString;

    private readonly Action<SqlDatabaseConnectionChangeEvent>[] _connectionChangeCallbacks;

    internal SqlitePersistentDatabase(
        string connectionString,
        SqliteDatabaseBuilder builder,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        Version version)
        : base( builder, versionRecordsQuery, version )
    {
        _connectionString = connectionString;
        _connectionChangeCallbacks = builder.ConnectionChanges.GetCallbacksArray();
    }

    [Pure]
    public override Microsoft.Data.Sqlite.SqliteConnection Connect()
    {
        var result = new SqliteConnection( _connectionString ) { ChangeCallbacks = _connectionChangeCallbacks };
        result.Open();
        return result;
    }

    [Pure]
    public override async ValueTask<Microsoft.Data.Sqlite.SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var result = new SqliteConnection( _connectionString ) { ChangeCallbacks = _connectionChangeCallbacks };
        await result.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return result;
    }
}
