using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderConstructError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderConstructError(
        ParsedExpressionBuilderErrorType type,
        object construct,
        StringSegment? token = null,
        Exception? exception = null)
        : base( type, token )
    {
        Construct = construct;
        Exception = exception;
    }

    public object Construct { get; }
    public Exception? Exception { get; }

    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, construct of type {Construct.GetType().GetDebugString()}";
        if ( Exception is null )
            return headerText;

        return $"{headerText}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
