using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionBoundArguments<TArg> : IReadOnlyCollection<KeyValuePair<StringSegment, TArg?>>
{
    public static readonly ParsedExpressionBoundArguments<TArg> Empty = new ParsedExpressionBoundArguments<TArg>(
        new Dictionary<StringSegment, TArg?>() );

    private readonly IReadOnlyDictionary<StringSegment, TArg?> _map;

    public ParsedExpressionBoundArguments(IEnumerable<KeyValuePair<StringSegment, TArg?>> map)
    {
        _map = new Dictionary<StringSegment, TArg?>( map );
    }

    internal ParsedExpressionBoundArguments(IReadOnlyDictionary<StringSegment, TArg?> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    public bool Contains(StringSegment name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public bool TryGetValue(StringSegment name, out TArg? result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public IEnumerator<KeyValuePair<StringSegment, TArg?>> GetEnumerator()
    {
        return _map.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
