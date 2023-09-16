using System;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlTypeCastExpressionNode : SqlExpressionNode
{
    internal SqlTypeCastExpressionNode(SqlExpressionNode value, Type targetType)
        : base( SqlNodeType.TypeCast )
    {
        Value = value;
        TargetType = targetType;
    }

    public SqlExpressionNode Value { get; }
    public Type TargetType { get; }
}
