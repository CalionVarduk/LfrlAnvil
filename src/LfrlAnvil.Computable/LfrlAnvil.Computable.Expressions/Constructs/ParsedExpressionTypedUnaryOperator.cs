using System;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a typed unary operator construct.
/// </summary>
public abstract class ParsedExpressionTypedUnaryOperator : ParsedExpressionUnaryOperator
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypedUnaryOperator"/> instance.
    /// </summary>
    /// <param name="argumentType">Argument's type.</param>
    protected ParsedExpressionTypedUnaryOperator(Type argumentType)
    {
        ArgumentType = argumentType;
    }

    /// <summary>
    /// Argument's type.
    /// </summary>
    public Type ArgumentType { get; }
}
