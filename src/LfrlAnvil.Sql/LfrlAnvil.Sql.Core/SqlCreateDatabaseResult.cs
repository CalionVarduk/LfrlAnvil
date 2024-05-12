using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents the result of an <see cref="ISqlDatabase"/> creation attempt.
/// </summary>
/// <typeparam name="TDatabase">SQL database type.</typeparam>
public readonly struct SqlCreateDatabaseResult<TDatabase>
    where TDatabase : ISqlDatabase
{
    private readonly SqlDatabaseVersionHistory.DatabaseComparisonResult _versions;
    private readonly int _appliedVersionCount;

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseResult{TDatabase}"/> instance.
    /// </summary>
    /// <param name="database">Created SQL database.</param>
    /// <param name="exception">Optional error that occurred during version application.</param>
    /// <param name="versions">Result of a comparison of previously applied versions to the desired versions.</param>
    /// <param name="appliedVersionCount">Number of new versions successfully applied to the database.</param>
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

    /// <summary>
    /// Created SQL database.
    /// </summary>
    public TDatabase Database { get; }

    /// <summary>
    /// Optional error that occurred during version application.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Previous version of the database.
    /// </summary>
    public Version OldVersion => _versions.Current;

    /// <summary>
    /// New version of the database.
    /// </summary>
    public Version NewVersion => Database.Version;

    /// <summary>
    /// Versions applied to the database before the creation of <see cref="Database"/> instance.
    /// </summary>
    public ReadOnlySpan<ISqlDatabaseVersion> OriginalVersions => _versions.Committed;

    /// <summary>
    /// Versions applied to the database during the creation of <see cref="Database"/> instance.
    /// </summary>
    public ReadOnlySpan<ISqlDatabaseVersion> CommittedVersions => _versions.Uncommitted.Slice( 0, _appliedVersionCount );

    /// <summary>
    /// Versions that haven't been applied to the database yet.
    /// </summary>
    public ReadOnlySpan<ISqlDatabaseVersion> PendingVersions => _versions.Uncommitted.Slice( _appliedVersionCount );

    /// <summary>
    /// Converts the <paramref name="result"/> to base <see cref="ISqlDatabase"/> type.
    /// </summary>
    /// <param name="result">Result to convert.</param>
    /// <returns>New <see cref="SqlCreateDatabaseResult{TDatabase}"/> instance.</returns>
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
