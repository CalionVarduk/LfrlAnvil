using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Errors;

public sealed class MathExpressionBuilderConstructError : MathExpressionBuilderError
{
    internal MathExpressionBuilderConstructError(
        MathExpressionBuilderErrorType type,
        IMathExpressionConstruct construct,
        StringSlice? token = null,
        Exception? exception = null)
        : base( type, token )
    {
        Construct = construct;
        Exception = exception;
    }

    public IMathExpressionConstruct Construct { get; }
    public Exception? Exception { get; }

    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, construct of type {Construct.GetType().FullName}";
        if ( Exception is null )
            return headerText;

        return $"{headerText}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
