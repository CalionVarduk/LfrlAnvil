using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public class ParsedExpressionBuilderMissingMemberError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingMemberError(
        ParsedExpressionBuilderErrorType type,
        StringSlice token,
        Type targetType)
        : base( type, token )
    {
        TargetType = targetType;
    }

    public Type TargetType { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, target type: {TargetType.FullName}";
    }
}
