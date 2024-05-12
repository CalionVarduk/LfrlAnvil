namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL foreign key constraint.
/// </summary>
public interface ISqlForeignKey : ISqlConstraint
{
    /// <summary>
    /// SQL index referenced by this foreign key.
    /// </summary>
    ISqlIndex ReferencedIndex { get; }

    /// <summary>
    /// SQL index that this foreign key originates from.
    /// </summary>
    ISqlIndex OriginIndex { get; }

    /// <summary>
    /// Specifies this foreign key's on delete behavior.
    /// </summary>
    ReferenceBehavior OnDeleteBehavior { get; }

    /// <summary>
    /// Specifies this foreign key's on update behavior.
    /// </summary>
    ReferenceBehavior OnUpdateBehavior { get; }
}
