using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderConstructError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderConstructError(
        ParsedExpressionBuilderErrorType type,
        IParsedExpressionConstruct construct,
        StringSlice? token = null,
        Exception? exception = null)
        : base( type, token )
    {
        Construct = construct;
        Exception = exception;
    }

    public IParsedExpressionConstruct Construct { get; }
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
