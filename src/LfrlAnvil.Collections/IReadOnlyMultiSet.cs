using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only multi set.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IReadOnlyMultiSet<T> : IReadOnlySet<Pair<T, int>>
    where T : notnull
{
    /// <summary>
    /// Gets the full count of all elements, including repetitions.
    /// </summary>
    long FullCount { get; }

    /// <summary>
    /// Gets all elements, including repetitions.
    /// </summary>
    IEnumerable<T> Items { get; }

    /// <summary>
    /// Gets all unique elements.
    /// </summary>
    IEnumerable<T> DistinctItems { get; }

    /// <summary>
    /// Element comparer.
    /// </summary>
    IEqualityComparer<T> Comparer { get; }

    /// <summary>
    /// Checks whether or not the provided <paramref name="item"/> exists in this set.
    /// </summary>
    /// <param name="item">Element to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="item"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(T item);

    /// <summary>
    /// Checks whether or not the provided <paramref name="item"/> exists in this set with a minimum number of repetitions.
    /// </summary>
    /// <param name="item">Element to check.</param>
    /// <param name="multiplicity">Expected minimum number of repetitions.</param>
    /// <returns>
    /// <b>true</b> when the provided <paramref name="item"/> exists with a minimum number of repetitions, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    bool Contains(T item, int multiplicity);

    /// <summary>
    /// Returns the number of repetitions associated with the provided <paramref name="item"/>.
    /// </summary>
    /// <param name="item">Element to get the number of repetitions for.</param>
    /// <returns>Number of <paramref name="item"/> repetitions.</returns>
    [Pure]
    int GetMultiplicity(T item);
}
