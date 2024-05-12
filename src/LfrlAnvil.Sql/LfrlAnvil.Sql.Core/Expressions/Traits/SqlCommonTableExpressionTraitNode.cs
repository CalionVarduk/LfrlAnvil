using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single common table expression trait.
/// </summary>
public sealed class SqlCommonTableExpressionTraitNode : SqlTraitNode
{
    internal SqlCommonTableExpressionTraitNode(SqlCommonTableExpressionNode[] commonTableExpressions)
        : base( SqlNodeType.CommonTableExpressionTrait )
    {
        CommonTableExpressions = commonTableExpressions;
    }

    /// <summary>
    /// Collection of common table expressions.
    /// </summary>
    public ReadOnlyArray<SqlCommonTableExpressionNode> CommonTableExpressions { get; }

    /// <summary>
    /// Specifies whether or not <see cref="CommonTableExpressions"/> contains at least one <see cref="SqlCommonTableExpressionNode"/>
    /// that is marked as recursive.
    /// </summary>
    public bool ContainsRecursive
    {
        get
        {
            foreach ( var cte in CommonTableExpressions )
            {
                if ( cte.IsRecursive )
                    return true;
            }

            return false;
        }
    }
}
