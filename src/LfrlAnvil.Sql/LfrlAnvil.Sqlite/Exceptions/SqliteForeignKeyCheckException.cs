using System;
using System.Collections.Generic;

namespace LfrlAnvil.Sqlite.Exceptions;

public class SqliteForeignKeyCheckException : InvalidOperationException
{
    public SqliteForeignKeyCheckException(Version version, IReadOnlySet<string> failedTableNames)
        : base( Resources.ForeignKeyCheckFailure( version, failedTableNames ) )
    {
        Version = version;
        FailedTableNames = failedTableNames;
    }

    public Version Version { get; }
    public IReadOnlySet<string> FailedTableNames { get; }
}
