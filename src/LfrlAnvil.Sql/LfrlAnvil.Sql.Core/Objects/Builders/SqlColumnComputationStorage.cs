namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents the type of computed SQL column's storage.
/// </summary>
public enum SqlColumnComputationStorage : byte
{
    /// <summary>
    /// Specifies that computed values are calculated every time they are read from the database.
    /// </summary>
    Virtual = 0,

    /// <summary>
    /// Specifies that computed values are stored in the database and updated on every relevant change.
    /// </summary>
    Stored = 1
}
