using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpression<TArg, out TResult>
{
    string Input { get; }
    Expression Body { get; }
    ParameterExpression Parameter { get; }
    ParsedExpressionUnboundArguments UnboundArguments { get; }
    ParsedExpressionBoundArguments<TArg> BoundArguments { get; }
    ParsedExpressionDiscardedArguments DiscardedArguments { get; }

    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<string, TArg?>> arguments);

    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<string, TArg?>[] arguments);

    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<StringSegment, TArg?>> arguments);

    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<StringSegment, TArg?>[] arguments);

    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<int, TArg?>> arguments);

    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<int, TArg?>[] arguments);

    [Pure]
    IParsedExpressionDelegate<TArg, TResult> Compile();
}
