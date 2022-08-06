using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Errors;

public sealed class MathExpressionBuilderMissingUnaryOperatorError : MathExpressionBuilderError
{
    internal MathExpressionBuilderMissingUnaryOperatorError(MathExpressionBuilderErrorType type, StringSlice token, Type argumentType)
        : base( type, token )
    {
        ArgumentType = argumentType;
    }

    public Type ArgumentType { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, argument type {ArgumentType.FullName}";
    }
}
