using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

public readonly struct SqlCreateDatabaseResult<TDatabase>
    where TDatabase : ISqlDatabase
{
    private readonly SqlDatabaseVersionHistory.DatabaseComparisonResult _versions;
    private readonly int _appliedVersionCount;

    public SqlCreateDatabaseResult(
        TDatabase database,
        Exception? exception,
        SqlDatabaseVersionHistory.DatabaseComparisonResult versions,
        int appliedVersionCount)
    {
        Assume.IsGreaterThanOrEqualTo( appliedVersionCount, 0 );
        Database = database;
        Exception = exception;
        _versions = versions;
        _appliedVersionCount = appliedVersionCount;
    }

    public TDatabase Database { get; }
    public Exception? Exception { get; }
    public Version OldVersion => _versions.Current;
    public Version NewVersion => Database.Version;
    public ReadOnlySpan<ISqlDatabaseVersion> OriginalVersions => _versions.Committed;
    public ReadOnlySpan<ISqlDatabaseVersion> CommittedVersions => _versions.Uncommitted.Slice( 0, _appliedVersionCount );
    public ReadOnlySpan<ISqlDatabaseVersion> PendingVersions => _versions.Uncommitted.Slice( _appliedVersionCount );

    [Pure]
    public static implicit operator SqlCreateDatabaseResult<ISqlDatabase>(SqlCreateDatabaseResult<TDatabase> result)
    {
        return new SqlCreateDatabaseResult<ISqlDatabase>(
            result.Database,
            result.Exception,
            result._versions,
            result._appliedVersionCount );
    }
}
