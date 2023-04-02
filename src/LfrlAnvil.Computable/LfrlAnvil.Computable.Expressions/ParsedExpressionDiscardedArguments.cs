using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionDiscardedArguments : IReadOnlyCollection<StringSegment>
{
    public static readonly ParsedExpressionDiscardedArguments
        Empty = new ParsedExpressionDiscardedArguments( new HashSet<StringSegment>() );

    private readonly IReadOnlySet<StringSegment> _set;

    public ParsedExpressionDiscardedArguments(IEnumerable<StringSegment> set)
    {
        _set = new HashSet<StringSegment>( set );
    }

    internal ParsedExpressionDiscardedArguments(IReadOnlySet<StringSegment> set)
    {
        _set = set;
    }

    public int Count => _set.Count;

    [Pure]
    public bool Contains(StringSegment name)
    {
        return _set.Contains( name );
    }

    [Pure]
    public IEnumerator<StringSegment> GetEnumerator()
    {
        return _set.GetEnumerator();
    }

    [Pure]
    internal ParsedExpressionDiscardedArguments AddTo(HashSet<StringSegment> other)
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
