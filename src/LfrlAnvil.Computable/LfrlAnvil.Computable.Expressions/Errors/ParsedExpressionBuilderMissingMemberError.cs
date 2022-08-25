using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMissingMemberError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingMemberError(
        ParsedExpressionBuilderErrorType type,
        StringSlice token,
        Type targetType,
        IReadOnlyList<Type> parameterTypes)
        : base( type, token )
    {
        TargetType = targetType;
        ParameterTypes = parameterTypes;
    }

    public Type TargetType { get; }
    public IReadOnlyList<Type> ParameterTypes { get; }

    [Pure]
    public override string ToString()
    {
        var baseText = $"{base.ToString()}, target type: {TargetType.FullName}";
        if ( ParameterTypes.Count == 0 )
            return baseText;

        return $"{baseText}, parameter types: [{string.Join( ", ", ParameterTypes.Select( p => p.FullName ) )}]";
    }
}
