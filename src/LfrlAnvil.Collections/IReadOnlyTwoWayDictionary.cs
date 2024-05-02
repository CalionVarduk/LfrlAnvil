using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only collection of two-way (forward, reverse) pairs.
/// </summary>
/// <typeparam name="T1">First value type.</typeparam>
/// <typeparam name="T2">Second value type.</typeparam>
public interface IReadOnlyTwoWayDictionary<T1, T2> : IReadOnlyCollection<Pair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    /// <summary>
    /// Represents the <typeparamref name="T1"/> => <typeparamref name="T2"/> read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<T1, T2> Forward { get; }

    /// <summary>
    /// Represents the <typeparamref name="T2"/> => <typeparamref name="T1"/> read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<T2, T1> Reverse { get; }

    /// <summary>
    /// Forward key equality comparer.
    /// </summary>
    IEqualityComparer<T1> ForwardComparer { get; }

    /// <summary>
    /// Reverse key equality comparer.
    /// </summary>
    IEqualityComparer<T2> ReverseComparer { get; }

    /// <summary>
    /// Checks whether or not the provided pair exists.
    /// </summary>
    /// <param name="first">First value.</param>
    /// <param name="second">Second value.</param>
    /// <returns><b>true</b> when pair exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(T1 first, T2 second);
}
