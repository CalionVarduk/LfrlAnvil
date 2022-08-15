using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMissingUnaryOperatorError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingUnaryOperatorError(ParsedExpressionBuilderErrorType type, StringSlice token, Type argumentType)
        : base( type, token )
    {
        ArgumentType = argumentType;
    }

    public Type ArgumentType { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, argument type: {ArgumentType.FullName}";
    }
}
