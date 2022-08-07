using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class MathExpressionDelegate<TArg, TResult> : IMathExpressionDelegate<TArg, TResult>
{
    private readonly IReadOnlyDictionary<StringSlice, int> _argumentIndexes;

    internal MathExpressionDelegate(Func<TArg?[], TResult> @delegate, IReadOnlyDictionary<StringSlice, int> argumentIndexes)
    {
        Delegate = @delegate;
        _argumentIndexes = argumentIndexes;
    }

    public Func<TArg?[], TResult> Delegate { get; }

    [Pure]
    public int GetArgumentCount()
    {
        return _argumentIndexes.Count;
    }

    [Pure]
    public IEnumerable<ReadOnlyMemory<char>> GetArgumentNames()
    {
        return _argumentIndexes.Select( kv => kv.Key.AsMemory() );
    }

    [Pure]
    public bool ContainsArgument(string argumentName)
    {
        return ContainsArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public bool ContainsArgument(ReadOnlyMemory<char> argumentName)
    {
        return ContainsArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public int GetArgumentIndex(string argumentName)
    {
        return GetArgumentIndex( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public int GetArgumentIndex(ReadOnlyMemory<char> argumentName)
    {
        return GetArgumentIndex( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public ReadOnlyMemory<char> GetArgumentName(int index)
    {
        foreach ( var (name, i) in _argumentIndexes )
        {
            if ( index != i )
                continue;

            return name.AsMemory();
        }

        return string.Empty.AsMemory();
    }

    [Pure]
    public TResult Invoke(params TArg?[] arguments)
    {
        if ( _argumentIndexes.Count != arguments.Length )
            throw new InvalidMathExpressionArgumentCountException( arguments.Length, _argumentIndexes.Count, nameof( arguments ) );

        return Delegate( arguments );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool ContainsArgument(StringSlice argumentName)
    {
        return _argumentIndexes.ContainsKey( argumentName );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int GetArgumentIndex(StringSlice argumentName)
    {
        return _argumentIndexes.TryGetValue( argumentName, out var index ) ? index : -1;
    }
}
