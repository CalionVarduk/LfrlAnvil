using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sqlite.Exceptions;

internal static class Resources
{
    internal const string ConnectionStringForInMemoryDatabaseIsImmutable = "Connection string for :memory: SQLite database is immutable.";

    internal const string ConnectionForClosedInMemoryDatabaseCannotBeReopened =
        "Connection for closed :memory: SQLite database cannot be reopened.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ForeignKeyCheckFailure(Version version, IReadOnlySet<string> failedTableNames)
    {
        var headerText = $"Foreign key check for version {version} failed for {failedTableNames.Count} table(s):";
        var tablesText = string.Join( Environment.NewLine, failedTableNames.Select( (n, i) => $"{i + 1}. \"{n}\"" ) );
        return $"{headerText}{Environment.NewLine}{tablesText}";
    }
}
