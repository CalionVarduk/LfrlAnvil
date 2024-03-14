using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlCommonTableExpressionTraitNode : SqlTraitNode
{
    internal SqlCommonTableExpressionTraitNode(SqlCommonTableExpressionNode[] commonTableExpressions)
        : base( SqlNodeType.CommonTableExpressionTrait )
    {
        CommonTableExpressions = commonTableExpressions;
    }

    public ReadOnlyArray<SqlCommonTableExpressionNode> CommonTableExpressions { get; }

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
