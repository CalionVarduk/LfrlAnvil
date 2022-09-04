using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionDiscardedArguments : IReadOnlyCollection<ReadOnlyMemory<char>>
{
    public static readonly ParsedExpressionDiscardedArguments Empty = new ParsedExpressionDiscardedArguments( new HashSet<StringSlice>() );

    private readonly IReadOnlySet<StringSlice> _set;

    public ParsedExpressionDiscardedArguments(IEnumerable<ReadOnlyMemory<char>> set)
    {
        _set = new HashSet<StringSlice>( set.Select( StringSlice.Create ) );
    }

    internal ParsedExpressionDiscardedArguments(IReadOnlySet<StringSlice> set)
    {
        _set = set;
    }

    public int Count => _set.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsMemory() );
    }

    [Pure]
    public bool Contains(ReadOnlyMemory<char> name)
    {
        return _set.Contains( StringSlice.Create( name ) );
    }

    [Pure]
    public IEnumerator<ReadOnlyMemory<char>> GetEnumerator()
    {
        return _set.Select( n => n.AsMemory() ).GetEnumerator();
    }

    [Pure]
    internal ParsedExpressionDiscardedArguments AddTo(HashSet<StringSlice> other)
    {
        foreach ( var name in _set )
            other.Add( name );

        return new ParsedExpressionDiscardedArguments( other );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
