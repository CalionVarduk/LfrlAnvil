using System;

namespace LfrlAnvil.Sql.Versioning;

public sealed record SqlDatabaseVersionRecord(
    int Ordinal,
    Version Version,
    string Description,
    DateTime CommitDateUtc,
    TimeSpan CommitDuration
);
