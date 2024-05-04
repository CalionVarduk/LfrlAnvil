using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to a missing function.
/// </summary>
public sealed class ParsedExpressionBuilderMissingFunctionError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingFunctionError(
        ParsedExpressionBuilderErrorType type,
        StringSegment token,
        IReadOnlyList<Type> parameterTypes)
        : base( type, token )
    {
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// Function's parameter types.
    /// </summary>
    public IReadOnlyList<Type> ParameterTypes { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderMissingFunctionError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, parameter types: [{string.Join( ", ", ParameterTypes.Select( static p => p.GetDebugString() ) )}]";
    }
}
