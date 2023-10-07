using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePersistentDatabase : SqliteDatabase
{
    [DebuggerBrowsable( DebuggerBrowsableState.Never )]
    private readonly string _connectionString;

    private readonly Action<SqlDatabaseConnectionChangeEvent>[] _connectionChangeCallbacks;

    internal SqlitePersistentDatabase(
        string connectionString,
        SqliteDatabaseBuilder builder,
        Func<SqliteCommand, List<SqlDatabaseVersionRecord>> versionRecordsReader,
        Version version)
        : base( builder, versionRecordsReader, version )
    {
        _connectionString = connectionString;
        _connectionChangeCallbacks = builder.ConnectionChanges.Callbacks.ToArray();
    }

    [Pure]
    public override Microsoft.Data.Sqlite.SqliteConnection Connect()
    {
        var result = new SqliteConnection( _connectionString ) { ChangeCallbacks = _connectionChangeCallbacks };
        result.Open();
        return result;
    }
}
