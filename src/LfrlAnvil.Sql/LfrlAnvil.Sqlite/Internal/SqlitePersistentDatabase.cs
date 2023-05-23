using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePersistentDatabase : SqliteDatabase
{
    [DebuggerBrowsable( DebuggerBrowsableState.Never )]
    private readonly string _connectionString;

    internal SqlitePersistentDatabase(
        string connectionString,
        SqliteDatabaseBuilder builder,
        Func<SqliteCommand, List<SqlDatabaseVersionRecord>> versionRecordsReader,
        Version version)
        : base( builder, versionRecordsReader, version )
    {
        _connectionString = connectionString;
    }

    [Pure]
    public override SqliteConnection Connect()
    {
        var result = new SqliteConnection( _connectionString );
        result.Open();
        return result;
    }
}
