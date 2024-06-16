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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a collection of named bound arguments of <see cref="IParsedExpression{TArg,TResult}"/>.
/// </summary>
public sealed class ParsedExpressionBoundArguments<TArg> : IReadOnlyCollection<KeyValuePair<StringSegment, TArg?>>
{
    /// <summary>
    /// Represents an empty collection of named bound arguments.
    /// </summary>
    public static readonly ParsedExpressionBoundArguments<TArg> Empty = new ParsedExpressionBoundArguments<TArg>(
        new Dictionary<StringSegment, TArg?>() );

    private readonly IReadOnlyDictionary<StringSegment, TArg?> _map;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBoundArguments{Targ}"/> instance.
    /// </summary>
    /// <param name="map">Source collection.</param>
    public ParsedExpressionBoundArguments(IEnumerable<KeyValuePair<StringSegment, TArg?>> map)
    {
        _map = new Dictionary<StringSegment, TArg?>( map );
    }

    internal ParsedExpressionBoundArguments(IReadOnlyDictionary<StringSegment, TArg?> map)
    {
        _map = map;
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <summary>
    /// Checks whether or not this collection contains an argument with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when an argument exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(StringSegment name)
    {
        return _map.ContainsKey( name );
    }

    /// <summary>
    /// Attempts to return a value associated with an argument with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="result"><b>out</b> parameter that returns a value associated with the provided argument.</param>
    /// <returns><b>true</b> when an argument exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool TryGetValue(StringSegment name, out TArg? result)
    {
        return _map.TryGetValue( name, out result );
    }

    /// <inheritdoc />
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
