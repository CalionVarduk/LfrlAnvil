using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionUnboundArguments : IReadOnlyCollection<KeyValuePair<StringSlice, int>>
{
    public static readonly ParsedExpressionUnboundArguments Empty = new ParsedExpressionUnboundArguments(
        new Dictionary<StringSlice, int>() );

    private readonly IReadOnlyDictionary<StringSlice, int> _indexes;
    private readonly StringSlice[] _names;

    public ParsedExpressionUnboundArguments(IEnumerable<KeyValuePair<StringSlice, int>> map)
    {
        _indexes = new Dictionary<StringSlice, int>( map );
        _names = CreateNames( _indexes );
    }

    internal ParsedExpressionUnboundArguments(IReadOnlyDictionary<StringSlice, int> indexes)
    {
        _indexes = indexes;
        _names = CreateNames( _indexes );
    }

    public int Count => _indexes.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsSlice() );
    }

    [Pure]
    public bool Contains(StringSlice name)
    {
        return _indexes.ContainsKey( name );
    }

    [Pure]
    public int GetIndex(string name)
    {
        return GetIndex( name.AsSlice() );
    }

    [Pure]
    public int GetIndex(StringSlice name)
    {
        return _indexes.TryGetValue( name, out var index ) ? index : -1;
    }

    [Pure]
    public StringSlice GetName(int index)
    {
        return _names[index];
    }

    [Pure]
    public IEnumerator<KeyValuePair<StringSlice, int>> GetEnumerator()
    {
        return _indexes.GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringSlice[] CreateNames(IReadOnlyDictionary<StringSlice, int> indexes)
    {
        var result = indexes.Count == 0 ? Array.Empty<StringSlice>() : new StringSlice[indexes.Count];
        foreach ( var (name, index) in indexes )
            result[index] = name;

        return result;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
