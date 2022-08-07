using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderResultTypeError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderResultTypeError(
        ParsedExpressionBuilderErrorType type,
        Type resultType,
        Type expectedType)
        : base( type )
    {
        ResultType = resultType;
        ExpectedType = expectedType;
    }

    public Type ResultType { get; }
    public Type ExpectedType { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, result type {ResultType.FullName}, expected type {ExpectedType.FullName}";
    }
}
