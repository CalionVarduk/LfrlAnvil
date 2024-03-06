using System;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlTypeCastExpressionNode : SqlExpressionNode
{
    internal SqlTypeCastExpressionNode(SqlExpressionNode value, Type targetType)
        : base( SqlNodeType.TypeCast )
    {
        Value = value;
        TargetType = targetType;
        TargetTypeDefinition = null;
    }

    internal SqlTypeCastExpressionNode(SqlExpressionNode value, ISqlColumnTypeDefinition targetTypeDefinition)
        : base( SqlNodeType.TypeCast )
    {
        Value = value;
        TargetType = targetTypeDefinition.RuntimeType;
        TargetTypeDefinition = targetTypeDefinition;
    }

    public SqlExpressionNode Value { get; }
    public Type TargetType { get; }
    public ISqlColumnTypeDefinition? TargetTypeDefinition { get; }
}
