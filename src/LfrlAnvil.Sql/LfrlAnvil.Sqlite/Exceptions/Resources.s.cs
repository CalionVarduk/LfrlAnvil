using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sqlite.Exceptions;

internal static class Resources
{
    internal const string ConnectionStringForPermanentDatabaseIsImmutable =
        "Connection string for permanent SQLite database is immutable.";

    internal const string ConnectionForClosedPermanentDatabaseCannotBeReopened =
        "Connection for closed permanent SQLite database cannot be reopened.";

    internal const string ConnectionStringToInMemoryDatabaseCannotBeModified =
        "Connection string to in-memory SQLite database cannot be modified.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ForeignKeyCheckFailure(Version version, IReadOnlySet<string> failedTableNames)
    {
        var headerText = $"Foreign key check for version {version} failed for {failedTableNames.Count} table(s):";
        var tablesText = string.Join( Environment.NewLine, failedTableNames.Select( (n, i) => $"{i + 1}. \"{n}\"" ) );
        return $"{headerText}{Environment.NewLine}{tablesText}";
    }
}
