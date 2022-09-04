using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionBoundArguments<TArg> : IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<char>, TArg?>>
{
    public static readonly ParsedExpressionBoundArguments<TArg> Empty = new ParsedExpressionBoundArguments<TArg>(
        new Dictionary<StringSlice, TArg?>() );

    private readonly IReadOnlyDictionary<StringSlice, TArg?> _map;

    public ParsedExpressionBoundArguments(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TArg?>> map)
    {
        _map = new Dictionary<StringSlice, TArg?>( map.Select( kv => KeyValuePair.Create( StringSlice.Create( kv.Key ), kv.Value ) ) );
    }

    internal ParsedExpressionBoundArguments(IReadOnlyDictionary<StringSlice, TArg?> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsMemory() );
    }

    [Pure]
    public bool Contains(ReadOnlyMemory<char> name)
    {
        return _map.ContainsKey( StringSlice.Create( name ) );
    }

    [Pure]
    public bool TryGetValue(string name, out TArg? result)
    {
        return TryGetValue( name.AsMemory(), out result );
    }

    [Pure]
    public bool TryGetValue(ReadOnlyMemory<char> name, out TArg? result)
    {
        return _map.TryGetValue( StringSlice.Create( name ), out result );
    }

    [Pure]
    public IEnumerator<KeyValuePair<ReadOnlyMemory<char>, TArg?>> GetEnumerator()
    {
        return _map.Select( kv => KeyValuePair.Create( kv.Key.AsMemory(), kv.Value ) ).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
