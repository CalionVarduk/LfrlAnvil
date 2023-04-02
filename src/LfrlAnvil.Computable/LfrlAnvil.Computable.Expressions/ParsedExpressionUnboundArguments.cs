using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionUnboundArguments : IReadOnlyCollection<KeyValuePair<StringSegment, int>>
{
    public static readonly ParsedExpressionUnboundArguments Empty = new ParsedExpressionUnboundArguments(
        new Dictionary<StringSegment, int>() );

    private readonly IReadOnlyDictionary<StringSegment, int> _indexes;
    private readonly StringSegment[] _names;

    public ParsedExpressionUnboundArguments(IEnumerable<KeyValuePair<StringSegment, int>> map)
    {
        _indexes = new Dictionary<StringSegment, int>( map );
        _names = CreateNames( _indexes );
    }

    internal ParsedExpressionUnboundArguments(IReadOnlyDictionary<StringSegment, int> indexes)
    {
        _indexes = indexes;
        _names = CreateNames( _indexes );
    }

    public int Count => _indexes.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsSegment() );
    }

    [Pure]
    public bool Contains(StringSegment name)
    {
        return _indexes.ContainsKey( name );
    }

    [Pure]
    public int GetIndex(string name)
    {
        return GetIndex( name.AsSegment() );
    }

    [Pure]
    public int GetIndex(StringSegment name)
    {
        return _indexes.TryGetValue( name, out var index ) ? index : -1;
    }

    [Pure]
    public StringSegment GetName(int index)
    {
        return _names[index];
    }

    [Pure]
    public IEnumerator<KeyValuePair<StringSegment, int>> GetEnumerator()
    {
        return _indexes.GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringSegment[] CreateNames(IReadOnlyDictionary<StringSegment, int> indexes)
    {
        var result = indexes.Count == 0 ? Array.Empty<StringSegment>() : new StringSegment[indexes.Count];
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
