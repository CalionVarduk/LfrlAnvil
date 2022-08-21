using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class FunctionCollection
{
    internal static readonly FunctionCollection Empty =
        new FunctionCollection( new Dictionary<FunctionSignatureKey, ParsedExpressionFunction>() );

    internal FunctionCollection(IReadOnlyDictionary<FunctionSignatureKey, ParsedExpressionFunction> functions)
    {
        Functions = functions;
    }

    internal IReadOnlyDictionary<FunctionSignatureKey, ParsedExpressionFunction> Functions { get; }

    [Pure]
    internal ParsedExpressionFunction? FindConstruct(IReadOnlyList<Expression> parameters)
    {
        return Functions.GetValueOrDefault( new FunctionSignatureKey( parameters ) );
    }
}
