namespace LfrlAnvil.Sql.Events;

/// <summary>
/// Represents a type of an SQL statement ran during <see cref="ISqlDatabase"/> creation.
/// </summary>
public enum SqlDatabaseFactoryStatementType : byte
{
    /// <summary>
    /// Represents a change to DB schema.
    /// </summary>
    Change = 0,

    /// <summary>
    /// Represents a statement related to the version history table.
    /// </summary>
    VersionHistory = 1,

    /// <summary>
    /// Represents an unspecified statement type.
    /// </summary>
    Other = 2
}
