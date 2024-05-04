using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a collection of named discarded arguments of <see cref="IParsedExpression{TArg,TResult}"/>.
/// </summary>
public sealed class ParsedExpressionDiscardedArguments : IReadOnlyCollection<StringSegment>
{
    /// <summary>
    /// Represents an empty collection of named discarded arguments.
    /// </summary>
    public static readonly ParsedExpressionDiscardedArguments
        Empty = new ParsedExpressionDiscardedArguments( new HashSet<StringSegment>() );

    private readonly IReadOnlySet<StringSegment> _set;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionDiscardedArguments"/> instance.
    /// </summary>
    /// <param name="set">Source collection.</param>
    public ParsedExpressionDiscardedArguments(IEnumerable<StringSegment> set)
    {
        _set = new HashSet<StringSegment>( set );
    }

    internal ParsedExpressionDiscardedArguments(IReadOnlySet<StringSegment> set)
    {
        _set = set;
    }

    /// <inheritdoc />
    public int Count => _set.Count;

    /// <summary>
    /// Checks whether or not this collection contains an argument with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when an argument exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(StringSegment name)
    {
        return _set.Contains( name );
    }

    /// <inheritdoc />
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
