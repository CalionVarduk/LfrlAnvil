using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionBoundArguments<TArg> : IReadOnlyCollection<KeyValuePair<StringSlice, TArg?>>
{
    public static readonly ParsedExpressionBoundArguments<TArg> Empty = new ParsedExpressionBoundArguments<TArg>(
        new Dictionary<StringSlice, TArg?>() );

    private readonly IReadOnlyDictionary<StringSlice, TArg?> _map;

    public ParsedExpressionBoundArguments(IEnumerable<KeyValuePair<StringSlice, TArg?>> map)
    {
        _map = new Dictionary<StringSlice, TArg?>( map );
    }

    internal ParsedExpressionBoundArguments(IReadOnlyDictionary<StringSlice, TArg?> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsSlice() );
    }

    [Pure]
    public bool Contains(StringSlice name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public bool TryGetValue(string name, out TArg? result)
    {
        return TryGetValue( name.AsSlice(), out result );
    }

    [Pure]
    public bool TryGetValue(StringSlice name, out TArg? result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public IEnumerator<KeyValuePair<StringSlice, TArg?>> GetEnumerator()
    {
        return _map.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
