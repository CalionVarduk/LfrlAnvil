using System;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a type cast expression.
/// </summary>
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

    /// <summary>
    /// Underlying value to cast to a different type.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Target runtime type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Optional <see cref="ISqlColumnTypeDefinition"/> instance that defines the target type.
    /// </summary>
    public ISqlColumnTypeDefinition? TargetTypeDefinition { get; }
}
