﻿using System.Text;

namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseLeftShiftExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseLeftShiftExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseLeftShift )
    {
        Type = SqlExpressionType.GetCommonType( left.Type, right.Type );
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "<<", Right, indent );
    }
}
