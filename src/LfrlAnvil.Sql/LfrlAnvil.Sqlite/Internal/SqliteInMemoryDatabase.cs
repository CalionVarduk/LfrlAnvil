﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteInMemoryDatabase : SqliteDatabase
{
    private readonly SqliteInMemoryConnection _connection;

    internal SqliteInMemoryDatabase(
        SqliteInMemoryConnection connection,
        SqliteDatabaseBuilder builder,
        Func<SqliteCommand, List<SqlDatabaseVersionRecord>> versionRecordsReader,
        Version version)
        : base( builder, versionRecordsReader, version )
    {
        _connection = connection;
    }

    public override void Dispose()
    {
        base.Dispose();
        _connection.Close();
    }

    [Pure]
    public override SqliteConnection Connect()
    {
        return _connection;
    }
}
