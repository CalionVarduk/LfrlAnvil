using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionDelegate<TArg, TResult> : IParsedExpressionDelegate<TArg, TResult>
{
    internal ParsedExpressionDelegate(Func<TArg?[], TResult> @delegate, ParsedExpressionUnboundArguments arguments)
    {
        Delegate = @delegate;
        Arguments = arguments;
    }

    public Func<TArg?[], TResult> Delegate { get; }
    public ParsedExpressionUnboundArguments Arguments { get; }

    [Pure]
    public TResult Invoke(params TArg?[] arguments)
    {
        if ( Arguments.Count != arguments.Length )
            throw new InvalidParsedExpressionArgumentCountException( arguments.Length, Arguments.Count, nameof( arguments ) );

        return Delegate( arguments );
    }
}
