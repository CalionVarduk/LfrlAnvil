using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions;

public interface IMathExpression<TArg, TResult>
{
    string Input { get; }
    Expression<Func<TArg?[], TResult>> Expression { get; }

    [Pure]
    int GetArgumentCount();

    [Pure]
    int GetUnboundArgumentCount();

    [Pure]
    int GetBoundArgumentCount();

    [Pure]
    IEnumerable<ReadOnlyMemory<char>> GetArgumentNames();

    [Pure]
    IEnumerable<ReadOnlyMemory<char>> GetUnboundArgumentNames();

    [Pure]
    IEnumerable<ReadOnlyMemory<char>> GetBoundArgumentNames();

    [Pure]
    bool ContainsArgument(string argumentName);

    [Pure]
    bool ContainsArgument(ReadOnlyMemory<char> argumentName);

    [Pure]
    bool ContainsUnboundArgument(string argumentName);

    [Pure]
    bool ContainsUnboundArgument(ReadOnlyMemory<char> argumentName);

    [Pure]
    bool ContainsBoundArgument(string argumentName);

    [Pure]
    bool ContainsBoundArgument(ReadOnlyMemory<char> argumentName);

    [Pure]
    int GetUnboundArgumentIndex(string argumentName);

    [Pure]
    int GetUnboundArgumentIndex(ReadOnlyMemory<char> argumentName);

    bool TryGetBoundArgumentValue(string argumentName, out TArg? result);
    bool TryGetBoundArgumentValue(ReadOnlyMemory<char> argumentName, out TArg? result);

    [Pure]
    ReadOnlyMemory<char> GetUnboundArgumentName(int index);

    [Pure]
    IMathExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<string, TArg?>> arguments);

    [Pure]
    IMathExpression<TArg, TResult> BindArguments(params KeyValuePair<string, TArg?>[] arguments);

    [Pure]
    IMathExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TArg?>> arguments);

    [Pure]
    IMathExpression<TArg, TResult> BindArguments(params KeyValuePair<ReadOnlyMemory<char>, TArg?>[] arguments);

    [Pure]
    IMathExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<int, TArg?>> arguments);

    [Pure]
    IMathExpression<TArg, TResult> BindArguments(params KeyValuePair<int, TArg?>[] arguments);

    [Pure]
    IMathExpressionDelegate<TArg, TResult> Compile();
}
