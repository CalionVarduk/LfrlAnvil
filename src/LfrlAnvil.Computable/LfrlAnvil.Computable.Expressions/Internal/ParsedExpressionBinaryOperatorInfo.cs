using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents an information about a binary operator construct.
/// </summary>
public readonly struct ParsedExpressionBinaryOperatorInfo
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBinaryOperatorInfo"/> instance.
    /// </summary>
    /// <param name="operatorType">Construct's type.</param>
    /// <param name="leftArgumentType">Left argument's type.</param>
    /// <param name="rightArgumentType">Right argument's type.</param>
    public ParsedExpressionBinaryOperatorInfo(Type operatorType, Type leftArgumentType, Type rightArgumentType)
    {
        OperatorType = operatorType;
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    /// <summary>
    /// Construct's type.
    /// </summary>
    public Type OperatorType { get; }

    /// <summary>
    /// Left argument's type.
    /// </summary>
    public Type LeftArgumentType { get; }

    /// <summary>
    /// Right argument's type.
    /// </summary>
    public Type RightArgumentType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBinaryOperatorInfo"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{OperatorType.GetDebugString()}({LeftArgumentType.GetDebugString()}, {RightArgumentType.GetDebugString()})";
    }
}
