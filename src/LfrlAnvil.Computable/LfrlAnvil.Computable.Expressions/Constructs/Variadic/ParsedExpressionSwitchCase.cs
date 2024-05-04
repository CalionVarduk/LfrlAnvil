using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents a switch case construct.
/// </summary>
public sealed class ParsedExpressionSwitchCase : ParsedExpressionVariadicFunction
{
    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, count: 2 );
        var body = parameters[^1];
        var result = Expression.SwitchCase( body, parameters.Take( parameters.Count - 1 ) );
        return Expression.Constant( result );
    }
}
