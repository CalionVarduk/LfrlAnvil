using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid array element expression.
/// </summary>
public class ParsedExpressionInvalidArrayElementException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionInvalidArrayElementException"/> instance.
    /// </summary>
    /// <param name="expectedType">Expected element type.</param>
    /// <param name="actualType">Actual element type.</param>
    public ParsedExpressionInvalidArrayElementException(Type expectedType, Type actualType)
        : base( Resources.InvalidArrayElementType( expectedType, actualType ) )
    {
        ExpectedType = expectedType;
        ActualType = actualType;
    }

    /// <summary>
    /// Expected element type.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Actual element type.
    /// </summary>
    public Type ActualType { get; }
}
