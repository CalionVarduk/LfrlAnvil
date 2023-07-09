﻿using System.Text;

namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlDivideExpressionNode : SqlExpressionNode
{
    internal SqlDivideExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Divide )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "/", Right, indent );
    }
}
