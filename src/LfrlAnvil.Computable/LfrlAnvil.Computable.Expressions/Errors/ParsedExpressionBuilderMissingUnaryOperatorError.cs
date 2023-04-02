using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMissingUnaryOperatorError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingUnaryOperatorError(ParsedExpressionBuilderErrorType type, StringSegment token, Type argumentType)
        : base( type, token )
    {
        ArgumentType = argumentType;
    }

    public Type ArgumentType { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, argument type: {ArgumentType.GetDebugString()}";
    }
}
