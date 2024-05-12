namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL object.
/// </summary>
public interface ISqlObject
{
    /// <summary>
    /// Object's name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Object's type.
    /// </summary>
    SqlObjectType Type { get; }

    /// <summary>
    /// Database that this object belongs to.
    /// </summary>
    ISqlDatabase Database { get; }
}
