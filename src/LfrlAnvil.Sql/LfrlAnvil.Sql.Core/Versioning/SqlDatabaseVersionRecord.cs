using System;

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents information about a single version applied to the database.
/// </summary>
/// <param name="Ordinal">Ordinal number of this version.</param>
/// <param name="Version">Identifier of this version.</param>
/// <param name="Description">Description of this version.</param>
/// <param name="CommitDateUtc">Specifies the date and time at which this version has been applied to the database.</param>
/// <param name="CommitDuration">Specifies the time it took to fully apply this version to the database.</param>
public sealed record SqlDatabaseVersionRecord(
    int Ordinal,
    Version Version,
    string Description,
    DateTime CommitDateUtc,
    TimeSpan CommitDuration
);
