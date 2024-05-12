namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents a type of record set join operation.
/// </summary>
public enum SqlJoinType : byte
{
    /// <summary>
    /// Represents an inner join.
    /// </summary>
    Inner = 0,

    /// <summary>
    /// Represents a left outer join.
    /// </summary>
    Left = 1,

    /// <summary>
    /// Represents a right outer join.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Represents a full outer join.
    /// </summary>
    Full = 3,

    /// <summary>
    /// Represents a cross join.
    /// </summary>
    Cross = 4
}
