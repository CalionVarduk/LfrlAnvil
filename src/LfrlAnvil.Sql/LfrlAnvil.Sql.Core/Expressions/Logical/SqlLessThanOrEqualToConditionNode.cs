﻿namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlLessThanOrEqualToConditionNode : SqlConditionNode
{
    internal SqlLessThanOrEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.LessThanOrEqualTo )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
