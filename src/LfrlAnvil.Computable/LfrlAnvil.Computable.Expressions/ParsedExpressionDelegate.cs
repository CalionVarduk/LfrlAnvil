using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions;

/// <inheritdoc />
public sealed class ParsedExpressionDelegate<TArg, TResult> : IParsedExpressionDelegate<TArg, TResult>
{
    internal ParsedExpressionDelegate(Func<TArg?[], TResult> @delegate, ParsedExpressionUnboundArguments arguments)
    {
        Delegate = @delegate;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public Func<TArg?[], TResult> Delegate { get; }

    /// <inheritdoc />
    public ParsedExpressionUnboundArguments Arguments { get; }

    /// <inheritdoc />
    [Pure]
    public TResult Invoke(params TArg?[] arguments)
    {
        if ( Arguments.Count != arguments.Length )
            throw new InvalidParsedExpressionArgumentCountException( arguments.Length, Arguments.Count, nameof( arguments ) );

        return Delegate( arguments );
    }
}
