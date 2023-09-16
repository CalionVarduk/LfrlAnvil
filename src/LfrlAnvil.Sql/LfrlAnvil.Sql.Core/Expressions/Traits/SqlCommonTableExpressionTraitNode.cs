using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlCommonTableExpressionTraitNode : SqlTraitNode
{
    internal SqlCommonTableExpressionTraitNode(SqlCommonTableExpressionNode[] commonTableExpressions)
        : base( SqlNodeType.CommonTableExpressionTrait )
    {
        CommonTableExpressions = commonTableExpressions;
    }

    public ReadOnlyMemory<SqlCommonTableExpressionNode> CommonTableExpressions { get; }
}
