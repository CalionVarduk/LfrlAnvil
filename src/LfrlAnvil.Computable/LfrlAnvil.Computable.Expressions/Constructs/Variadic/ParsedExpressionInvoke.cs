using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionInvoke : ParsedExpressionVariadicFunction
{
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 1, nameof( parameters ) );
        var target = parameters[0];
        // TODO: create constant from invocation if target & all parameters are constant expressions
        // TODO: add property to ctor that allows to disable constant resolution, enabled by default
        // null delegates will be ignored by this configuration due to how closures for inline delegates work
        return Expression.Invoke( target, parameters.Skip( 1 ) );
    }
}
