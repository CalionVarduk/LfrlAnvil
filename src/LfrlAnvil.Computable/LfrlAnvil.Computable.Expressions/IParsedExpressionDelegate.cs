using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpressionDelegate<in TArg, out TResult>
{
    Func<TArg?[], TResult> Delegate { get; }
    ParsedExpressionUnboundArguments Arguments { get; }

    [Pure]
    TResult Invoke(params TArg?[] arguments);
}
