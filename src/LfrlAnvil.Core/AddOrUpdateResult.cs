namespace LfrlAnvil;

/// <summary>
/// Represents a result of an operation that can either add a new element or update an existing one.
/// </summary>
public enum AddOrUpdateResult : byte
{
    /// <summary>
    /// Specifies that an operation ended with addition of a new element.
    /// </summary>
    Added = 0,

    /// <summary>
    /// Specifies that an operation ended with update of an existing element.
    /// </summary>
    Updated = 1
}
