using System;

namespace LfrlAnvil.Sqlite;

[Flags]
public enum SqliteUpsertOptions : byte
{
    Disabled = 0,
    Supported = 1,
    AllowEmptyConflictTarget = 2
}
