using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an unexpected expression.
/// </summary>
public class ParsedExpressionInvalidExpressionException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionInvalidExpressionException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="expression">Invalid expression.</param>
    public ParsedExpressionInvalidExpressionException(string message, Expression expression)
        : base( message )
    {
        Expression = expression;
    }

    /// <summary>
    /// Invalid expression.
    /// </summary>
    public Expression Expression { get; }
}
