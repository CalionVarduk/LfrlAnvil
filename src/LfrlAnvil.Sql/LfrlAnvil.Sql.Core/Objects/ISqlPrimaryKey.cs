namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL primary key constraint.
/// </summary>
public interface ISqlPrimaryKey : ISqlConstraint
{
    /// <summary>
    /// Underlying index that defines this primary key.
    /// </summary>
    ISqlIndex Index { get; }
}
