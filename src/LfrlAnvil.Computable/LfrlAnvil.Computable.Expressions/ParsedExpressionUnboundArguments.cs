// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a collection of named unbound arguments of <see cref="IParsedExpression{TArg,TResult}"/>.
/// </summary>
public sealed class ParsedExpressionUnboundArguments : IReadOnlyCollection<KeyValuePair<StringSegment, int>>
{
    /// <summary>
    /// Represents an empty collection of named unbound arguments.
    /// </summary>
    public static readonly ParsedExpressionUnboundArguments Empty = new ParsedExpressionUnboundArguments(
        new Dictionary<StringSegment, int>() );

    private readonly IReadOnlyDictionary<StringSegment, int> _indexes;
    private readonly StringSegment[] _names;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnboundArguments"/> instance.
    /// </summary>
    /// <param name="map">Source collection.</param>
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

    /// <inheritdoc />
    public int Count => _indexes.Count;

    /// <summary>
    /// Checks whether or not this collection contains an argument with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when an argument exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(StringSegment name)
    {
        return _indexes.ContainsKey( name );
    }

    /// <summary>
    /// Returns the 0-based position of an argument with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <returns>0-based position of an argument when it exists, otherwise <b>-1</b>.</returns>
    [Pure]
    public int GetIndex(StringSegment name)
    {
        return _indexes.TryGetValue( name, out var index ) ? index : -1;
    }

    /// <summary>
    /// Returns the name of an argument at the specified 0-based position.
    /// </summary>
    /// <param name="index">0-based index.</param>
    /// <returns>Name of an argument at the specified position.</returns>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is not valid.</exception>
    [Pure]
    public StringSegment GetName(int index)
    {
        return _names[index];
    }

    /// <inheritdoc />
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
