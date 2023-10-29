using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionInvoke : ParsedExpressionVariadicFunction
{
    public ParsedExpressionInvoke(bool foldConstantsWhenPossible = true)
    {
        FoldConstantsWhenPossible = foldConstantsWhenPossible;
    }

    public bool FoldConstantsWhenPossible { get; }

    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 1 );

        var target = parameters[0];
        var callParameters = parameters.Slice( 1 );

        return FoldConstantsWhenPossible &&
            target is ConstantExpression constantTarget &&
            callParameters.All( static p => p is ConstantExpression )
                ? ExpressionHelpers.CreateConstantDelegateInvocation( constantTarget, callParameters )
                : Expression.Invoke( target, callParameters );
    }
}
