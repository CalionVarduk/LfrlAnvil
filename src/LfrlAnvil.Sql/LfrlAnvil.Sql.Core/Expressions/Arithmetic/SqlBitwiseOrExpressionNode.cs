﻿namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseOrExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseOrExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseOr )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}