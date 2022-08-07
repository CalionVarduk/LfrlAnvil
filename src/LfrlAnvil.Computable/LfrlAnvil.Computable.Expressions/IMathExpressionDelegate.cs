using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions;

public interface IMathExpressionDelegate<in TArg, out TResult>
{
    Func<TArg?[], TResult> Delegate { get; }

    [Pure]
    int GetArgumentCount();

    [Pure]
    IEnumerable<ReadOnlyMemory<char>> GetArgumentNames();

    [Pure]
    bool ContainsArgument(string argumentName);

    [Pure]
    bool ContainsArgument(ReadOnlyMemory<char> argumentName);

    [Pure]
    int GetArgumentIndex(string argumentName);

    [Pure]
    int GetArgumentIndex(ReadOnlyMemory<char> argumentName);

    [Pure]
    ReadOnlyMemory<char> GetArgumentName(int index);

    [Pure]
    TResult Invoke(params TArg?[] arguments);
}
