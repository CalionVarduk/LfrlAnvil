using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an attempt to invoke an instance of a non-delegate type.
/// </summary>
public class ParsedExpressionNonInvocableTypeExpression : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionNonInvocableTypeExpression"/> instance.
    /// </summary>
    /// <param name="type">Non-invocable type.</param>
    public ParsedExpressionNonInvocableTypeExpression(Type type)
        : base( Resources.NonInvocableType( type ) )
    {
        Type = type;
    }

    /// <summary>
    /// Non-invocable type.
    /// </summary>
    public Type Type { get; }
}
