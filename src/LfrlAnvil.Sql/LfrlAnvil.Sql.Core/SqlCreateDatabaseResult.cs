using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

public readonly ref struct SqlCreateDatabaseResult<TDatabase>
    where TDatabase : ISqlDatabase
{
    public readonly TDatabase Database;
    public readonly Exception? Exception;
    public readonly Version OldVersion;
    public readonly Version NewVersion;
    public readonly ReadOnlySpan<SqlDatabaseVersion> CommittedVersions;
    public readonly ReadOnlySpan<SqlDatabaseVersion> PendingVersions;

    public SqlCreateDatabaseResult(
        TDatabase database,
        Exception? exception,
        Version oldVersion,
        Version newVersion,
        ReadOnlySpan<SqlDatabaseVersion> committedVersions,
        ReadOnlySpan<SqlDatabaseVersion> pendingVersions)
    {
        Database = database;
        Exception = exception;
        OldVersion = oldVersion;
        NewVersion = newVersion;
        CommittedVersions = committedVersions;
        PendingVersions = pendingVersions;
    }

    [Pure]
    public static implicit operator SqlCreateDatabaseResult<ISqlDatabase>(SqlCreateDatabaseResult<TDatabase> result)
    {
        return new SqlCreateDatabaseResult<ISqlDatabase>(
            result.Database,
            result.Exception,
            result.OldVersion,
            result.NewVersion,
            result.CommittedVersions,
            result.PendingVersions );
    }
}
