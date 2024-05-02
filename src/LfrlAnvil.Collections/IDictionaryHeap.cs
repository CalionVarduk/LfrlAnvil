using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic heap data structure with the ability to identify entries by keys.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IDictionaryHeap<TKey, TValue> : IReadOnlyDictionaryHeap<TKey, TValue>
{
    /// <summary>
    /// Removes and returns an entry currently at the top of this heap.
    /// </summary>
    /// <returns>Removed entry.</returns>
    /// <exception cref="IndexOutOfRangeException">When this heap is empty.</exception>
    TValue Extract();

    /// <summary>
    /// Attempt to remove and return an entry currently at the top of this heap if it is not empty.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns the removed entry.</param>
    /// <returns><b>true</b> when this heap was not empty and entry has been removed, otherwise <b>false</b>.</returns>
    bool TryExtract([MaybeNullWhen( false )] out TValue result);

    /// <summary>
    /// Adds a new entry to this heap.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry to add.</param>
    /// <exception cref="ArgumentException">When <paramref name="key"/> already exists in this heap.</exception>
    void Add(TKey key, TValue value);

    /// <summary>
    /// Attempts to add a new entry to this heap.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry to add.</param>
    /// <returns><b>true</b> when key did not exist and entry was added, otherwise <b>false</b>.</returns>
    bool TryAdd(TKey key, TValue value);

    /// <summary>
    /// Removes an entry associated with the specified <paramref name="key"/> from this heap.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <returns>Removed entry.</returns>
    /// <exception cref="ArgumentException">When key does not exist in this heap.</exception>
    TValue Remove(TKey key);

    /// <summary>
    /// Attempts to remove an entry associated with the specified <paramref name="key"/> from this heap.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="removed"><b>out</b> parameter that returns the removed entry.</param>
    /// <returns><b>true</b> when entry was removed, otherwise <b>false</b>.</returns>
    bool TryRemove(TKey key, [MaybeNullWhen( false )] out TValue removed);

    /// <summary>
    /// Removes an entry currently at the top of this heap.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">When this heap is empty.</exception>
    void Pop();

    /// <summary>
    /// Attempts to remove an entry currently at the top of the heap if it is not empty.
    /// </summary>
    /// <returns><b>true</b> when this heap was not empty and entry has been removed, otherwise <b>false</b>.</returns>
    bool TryPop();

    /// <summary>
    /// Returns and replaces an entry currently associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Replacement entry.</param>
    /// <returns>Removed entry.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="key"/> does not exist in this heap.</exception>
    TValue Replace(TKey key, TValue value);

    /// <summary>
    /// Attempt to return and replace an entry associated with the specified <paramref name="key"/> with a new entry if it exists.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Replacement entry.</param>
    /// <param name="replaced"><b>out</b> parameter that returns the removed entry.</param>
    /// <returns><b>true</b> when <paramref name="key"/> exists in this heap, otherwise <b>false</b>.</returns>
    bool TryReplace(TKey key, TValue value, [MaybeNullWhen( false )] out TValue replaced);

    /// <summary>
    /// Adds or replaces an entry associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry to add or replace with.</param>
    /// <returns>Removed entry if <paramref name="key"/> existed in this heap, otherwise the provided <paramref name="value"/>.</returns>
    TValue AddOrReplace(TKey key, TValue value);

    /// <summary>
    /// Removes all entries from this heap.
    /// </summary>
    void Clear();
}
