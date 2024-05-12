namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL constraint attached to a table.
/// </summary>
public interface ISqlConstraint : ISqlObject
{
    /// <summary>
    /// Table that this constraint is attached to.
    /// </summary>
    ISqlTable Table { get; }
}
