using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.TestExtensions.Sql.Mocks.System;

public readonly record struct ResultSet(string[] FieldNames, object?[][] Rows)
{
    [Pure]
    public static ResultSet Create(params SqlDatabaseVersionRecord[] records)
    {
        var fields = new[]
        {
            "Ordinal",
            "VersionMajor",
            "VersionMinor",
            "VersionBuild",
            "VersionRevision",
            "Description",
            "CommitDateUtc",
            "CommitDurationInTicks"
        };

        var rows = new object?[records.Length][];
        for ( var i = 0; i < rows.Length; ++i )
        {
            var r = records[i];
            var row = new object?[fields.Length];
            row[0] = r.Ordinal;
            row[1] = r.Version.Major;
            row[2] = r.Version.Minor;
            row[3] = r.Version.Build >= 0 ? r.Version.Build : null;
            row[4] = r.Version.Revision >= 0 ? r.Version.Revision : null;
            row[5] = r.Description;
            row[6] = r.CommitDateUtc;
            row[7] = r.CommitDuration.Ticks;
            rows[i] = row;
        }

        return new ResultSet( fields, rows );
    }
}
