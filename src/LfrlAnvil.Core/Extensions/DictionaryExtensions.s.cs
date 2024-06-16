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
using System.Collections.Generic;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IDictionary{TKey,TValue}"/> extension methods.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Reads a value associated with the specified <paramref name="key"/> from the <paramref name="dictionary"/>, if it exists.
    /// Otherwise adds a new entry to the <paramref name="dictionary"/> with default value and returns that value.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <returns>Value associated with the provided <paramref name="key"/>, if it exists, otherwise default value</returns>
    public static TValue? GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue?> dictionary, TKey key)
        where TKey : notnull
    {
        if ( dictionary.TryGetValue( key, out var value ) )
            return value;

        value = default;
        dictionary[key] = value;
        return value;
    }

    /// <summary>
    /// Reads a value associated with the specified <paramref name="key"/> from the <paramref name="dictionary"/>, if it exists.
    /// Otherwise adds a new entry to the <paramref name="dictionary"/> with the value returned
    /// by the <paramref name="defaultValueProvider"/> invocation and returns that value.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="defaultValueProvider">
    /// Entry's value provider. Gets invoked only when entry with the provided <paramref name="key"/>
    /// does not exist in the <paramref name="dictionary"/>.
    /// </param>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <returns>
    /// Value associated with the provided <paramref name="key"/>, if it exists,
    /// otherwise value returned by <paramref name="defaultValueProvider"/>.
    /// </returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        where TKey : notnull
    {
        if ( dictionary.TryGetValue( key, out var value ) )
            return value;

        value = defaultValueProvider();
        dictionary[key] = value;
        return value;
    }

    /// <summary>
    /// Reads a value associated with the specified <paramref name="key"/> from the <paramref name="dictionary"/>, if it exists.
    /// Otherwise adds a new entry to the <paramref name="dictionary"/> with the value returned
    /// by the <paramref name="defaultValueProvider"/> invocation and returns that value.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="defaultValueProvider">
    /// Entry's value provider. Gets invoked only when entry with the provided <paramref name="key"/>
    /// does not exist in the <paramref name="dictionary"/>.
    /// </param>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <returns>
    /// Value associated with the provided <paramref name="key"/>, if it exists,
    /// otherwise value returned by <paramref name="defaultValueProvider"/>.
    /// </returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Lazy<TValue> defaultValueProvider)
        where TKey : notnull
    {
        if ( dictionary.TryGetValue( key, out var value ) )
            return value;

        value = defaultValueProvider.Value;
        dictionary[key] = value;
        return value;
    }

    /// <summary>
    /// Adds a new entry or updates an existing one if <paramref name="key"/> already exists.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <returns>
    /// <see cref="AddOrUpdateResult.Added"/> when new entry has been added (provided <paramref name="key"/> did not exist),
    /// otherwise <see cref="AddOrUpdateResult.Updated"/>.
    /// </returns>
    public static AddOrUpdateResult AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if ( dictionary.TryAdd( key, value ) )
            return AddOrUpdateResult.Added;

        dictionary[key] = value;
        return AddOrUpdateResult.Updated;
    }

    /// <summary>
    /// Attempts to update an existing entry in the <paramref name="dictionary"/> with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <returns><b>true</b> when entry with the specified <paramref name="key"/> exists, otherwise <b>false</b>.</returns>
    public static bool TryUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if ( ! dictionary.ContainsKey( key ) )
            return false;

        dictionary[key] = value;
        return true;
    }
}
