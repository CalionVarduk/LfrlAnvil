using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionDiscardedArguments : IReadOnlyCollection<ReadOnlyMemory<char>>
{
    public static readonly ParsedExpressionDiscardedArguments Empty = new ParsedExpressionDiscardedArguments( new HashSet<StringSliceOld>() );

    private readonly IReadOnlySet<StringSliceOld> _set;

    public ParsedExpressionDiscardedArguments(IEnumerable<ReadOnlyMemory<char>> set)
    {
        _set = new HashSet<StringSliceOld>( set.Select( StringSliceOld.Create ) );
    }

    internal ParsedExpressionDiscardedArguments(IReadOnlySet<StringSliceOld> set)
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
        return _set.Contains( StringSliceOld.Create( name ) );
    }

    [Pure]
    public IEnumerator<ReadOnlyMemory<char>> GetEnumerator()
    {
        return _set.Select( n => n.AsMemory() ).GetEnumerator();
    }

    [Pure]
    internal ParsedExpressionDiscardedArguments AddTo(HashSet<StringSliceOld> other)
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
