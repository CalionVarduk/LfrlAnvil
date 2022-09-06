using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionDiscardedArguments : IReadOnlyCollection<StringSlice>
{
    public static readonly ParsedExpressionDiscardedArguments Empty = new ParsedExpressionDiscardedArguments( new HashSet<StringSlice>() );

    private readonly IReadOnlySet<StringSlice> _set;

    public ParsedExpressionDiscardedArguments(IEnumerable<StringSlice> set)
    {
        _set = new HashSet<StringSlice>( set );
    }

    internal ParsedExpressionDiscardedArguments(IReadOnlySet<StringSlice> set)
    {
        _set = set;
    }

    public int Count => _set.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsSlice() );
    }

    [Pure]
    public bool Contains(StringSlice name)
    {
        return _set.Contains( name );
    }

    [Pure]
    public IEnumerator<StringSlice> GetEnumerator()
    {
        return _set.GetEnumerator();
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
