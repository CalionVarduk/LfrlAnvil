using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a variadic function construct.
/// </summary>
public abstract class ParsedExpressionVariadicFunction
{
    /// <summary>
    /// Processes this construct.
    /// </summary>
    /// <param name="parameters">Parameters to process.</param>
    /// <returns>New <see cref="Expression"/>.</returns>
    [Pure]
    protected internal abstract Expression Process(IReadOnlyList<Expression> parameters);
}
