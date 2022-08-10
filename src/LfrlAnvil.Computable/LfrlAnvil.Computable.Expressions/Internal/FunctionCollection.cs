using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class FunctionCollection
{
    internal static readonly FunctionCollection Empty =
        new FunctionCollection( new Dictionary<FunctionSignatureKey, ParsedExpressionFunction>() );

    private readonly IReadOnlyDictionary<FunctionSignatureKey, ParsedExpressionFunction> _functions;

    internal FunctionCollection(IReadOnlyDictionary<FunctionSignatureKey, ParsedExpressionFunction> functions)
    {
        _functions = functions;
    }

    [Pure]
    internal ParsedExpressionFunction? FindConstruct(IReadOnlyList<Type> parameterTypes)
    {
        return _functions.GetValueOrDefault( new FunctionSignatureKey( parameterTypes ) );
    }
}
