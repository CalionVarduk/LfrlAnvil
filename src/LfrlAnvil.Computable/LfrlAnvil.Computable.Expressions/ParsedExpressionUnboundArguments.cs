using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionUnboundArguments : IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<char>, int>>
{
    public static readonly ParsedExpressionUnboundArguments Empty = new ParsedExpressionUnboundArguments(
        new Dictionary<StringSliceOld, int>() );

    private readonly IReadOnlyDictionary<StringSliceOld, int> _indexes;
    private readonly StringSliceOld[] _names;

    public ParsedExpressionUnboundArguments(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> map)
    {
        _indexes = new Dictionary<StringSliceOld, int>( map.Select( kv => KeyValuePair.Create( StringSliceOld.Create( kv.Key ), kv.Value ) ) );
        _names = CreateNames( _indexes );
    }

    internal ParsedExpressionUnboundArguments(IReadOnlyDictionary<StringSliceOld, int> indexes)
    {
        _indexes = indexes;
        _names = CreateNames( _indexes );
    }

    public int Count => _indexes.Count;

    [Pure]
    public bool Contains(string name)
    {
        return Contains( name.AsMemory() );
    }

    [Pure]
    public bool Contains(ReadOnlyMemory<char> name)
    {
        return _indexes.ContainsKey( StringSliceOld.Create( name ) );
    }

    [Pure]
    public int GetIndex(string name)
    {
        return GetIndex( name.AsMemory() );
    }

    [Pure]
    public int GetIndex(ReadOnlyMemory<char> name)
    {
        return _indexes.TryGetValue( StringSliceOld.Create( name ), out var index ) ? index : -1;
    }

    [Pure]
    public ReadOnlyMemory<char> GetName(int index)
    {
        return _names[index].AsMemory();
    }

    [Pure]
    public IEnumerator<KeyValuePair<ReadOnlyMemory<char>, int>> GetEnumerator()
    {
        return _indexes.Select( kv => KeyValuePair.Create( kv.Key.AsMemory(), kv.Value ) ).GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringSliceOld[] CreateNames(IReadOnlyDictionary<StringSliceOld, int> indexes)
    {
        var result = indexes.Count == 0 ? Array.Empty<StringSliceOld>() : new StringSliceOld[indexes.Count];
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
