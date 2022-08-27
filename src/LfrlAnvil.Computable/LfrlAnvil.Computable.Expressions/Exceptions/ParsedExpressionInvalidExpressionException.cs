using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionInvalidExpressionException : InvalidOperationException
{
    public ParsedExpressionInvalidExpressionException(string message, Expression expression)
        : base( message )
    {
        Expression = expression;
    }

    public Expression Expression { get; }
}
