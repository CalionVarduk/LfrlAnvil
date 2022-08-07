using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class MathExpressionBuilderExceptionError : MathExpressionBuilderError
{
    public MathExpressionBuilderExceptionError(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
