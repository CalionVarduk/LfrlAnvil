namespace LfrlAnvil.Sql;

/// <summary>
/// Represents how <see cref="ISqlDatabaseFactory"/> should handle version history table records.
/// </summary>
public enum SqlDatabaseVersionHistoryMode : byte
{
    /// <summary>
    /// Specifies that all version history table records should be included.
    /// </summary>
    AllRecords = 0,

    /// <summary>
    /// Specifies that only the last registered version history table record should be included.
    /// </summary>
    LastRecordOnly = 1
}
