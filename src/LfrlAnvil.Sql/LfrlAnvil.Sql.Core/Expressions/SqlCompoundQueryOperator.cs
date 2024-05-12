namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents a compound query operator.
/// </summary>
public enum SqlCompoundQueryOperator : byte
{
    /// <summary>
    /// Specifies a union operator. Only distinct records will be included.
    /// </summary>
    Union = 0,

    /// <summary>
    /// Specifies a union all operator. Non-distinct records will be included.
    /// </summary>
    UnionAll = 1,

    /// <summary>
    /// Specifies an intersect operator.
    /// </summary>
    Intersect = 2,

    /// <summary>
    /// Specifies an except operator.
    /// </summary>
    Except = 3
}
