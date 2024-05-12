namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a state of existence of an SQL object builder instance.
/// </summary>
public enum SqlObjectExistenceState : byte
{
    /// <summary>
    /// Specifies that an object has not been created or removed.
    /// </summary>
    Unchanged = 0,

    /// <summary>
    /// Specifies that an object has been created.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Specifies that an object has been removed.
    /// </summary>
    Removed = 2
}
