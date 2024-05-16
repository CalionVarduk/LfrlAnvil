using System;
using System.Collections.Generic;

namespace LfrlAnvil.Sqlite.Exceptions;

/// <summary>
/// Represents an error that occurred during foreign key constraint validation.
/// </summary>
/// <remarks>See <see cref="SqliteDatabaseFactoryOptions.AreForeignKeyChecksDisabled"/> for more information.</remarks>
public class SqliteForeignKeyCheckException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqliteForeignKeyCheckException"/> instance.
    /// </summary>
    /// <param name="version">Version's identifier.</param>
    /// <param name="failedTableNames">Collection of names of tables for which the foreign key constraint validation has failed.</param>
    public SqliteForeignKeyCheckException(Version version, IReadOnlySet<string> failedTableNames)
        : base( Resources.ForeignKeyCheckFailure( version, failedTableNames ) )
    {
        Version = version;
        FailedTableNames = failedTableNames;
    }

    /// <summary>
    /// Version's identifier.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Collection of names of tables for which the foreign key constraint validation has failed.
    /// </summary>
    public IReadOnlySet<string> FailedTableNames { get; }
}
