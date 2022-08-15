using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class ParsedExpressionVariadicFunction
{
    [Pure]
    protected internal abstract Expression Process(IReadOnlyList<Expression> parameters);
}
