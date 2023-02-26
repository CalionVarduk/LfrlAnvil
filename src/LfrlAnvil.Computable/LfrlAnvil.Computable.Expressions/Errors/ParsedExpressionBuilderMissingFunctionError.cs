using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMissingFunctionError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingFunctionError(
        ParsedExpressionBuilderErrorType type,
        StringSlice token,
        IReadOnlyList<Type> parameterTypes)
        : base( type, token )
    {
        ParameterTypes = parameterTypes;
    }

    public IReadOnlyList<Type> ParameterTypes { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, parameter types: [{string.Join( ", ", ParameterTypes.Select( static p => p.GetDebugString() ) )}]";
    }
}
