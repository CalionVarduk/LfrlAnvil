using System;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a typed binary operator construct.
/// </summary>
public abstract class ParsedExpressionTypedBinaryOperator : ParsedExpressionBinaryOperator
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypedBinaryOperator"/> instance.
    /// </summary>
    /// <param name="leftArgumentType">Left argument's type.</param>
    /// <param name="rightArgumentType">Right argument's type.</param>
    protected ParsedExpressionTypedBinaryOperator(Type leftArgumentType, Type rightArgumentType)
    {
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    /// <summary>
    /// Left argument's type.
    /// </summary>
    public Type LeftArgumentType { get; }

    /// <summary>
    /// Right argument's type.
    /// </summary>
    public Type RightArgumentType { get; }
}
