using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased collection of elements that belong to an <see cref="IReadOnlyCollectionVariable"/>.
/// </summary>
public interface ICollectionVariableElements : IEnumerable
{
    /// <summary>
    /// Number of elements in this collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Underlying collection of element keys.
    /// </summary>
    IEnumerable Keys { get; }

    /// <summary>
    /// Underlying collection of elements.
    /// </summary>
    IEnumerable Values { get; }

    /// <summary>
    /// Collection of keys of invalid elements.
    /// </summary>
    IEnumerable InvalidElementKeys { get; }

    /// <summary>
    /// Collection of keys of elements with warnings.
    /// </summary>
    IEnumerable WarningElementKeys { get; }

    /// <summary>
    /// Collection of keys of changed elements.
    /// </summary>
    IEnumerable ModifiedElementKeys { get; }
}

/// <summary>
/// Represents a generic collection of elements that belong to an <see cref="IReadOnlyCollectionVariable{TKey,TElement}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public interface ICollectionVariableElements<TKey, TElement> : ICollectionVariableElements, IReadOnlyDictionary<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Number of elements in this collection.
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// Underlying collection of element keys.
    /// </summary>
    new IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// Underlying collection of elements.
    /// </summary>
    new IReadOnlyCollection<TElement> Values { get; }

    /// <summary>
    /// Collection of keys of invalid elements.
    /// </summary>
    new IReadOnlySet<TKey> InvalidElementKeys { get; }

    /// <summary>
    /// Collection of keys of elements with warnings.
    /// </summary>
    new IReadOnlySet<TKey> WarningElementKeys { get; }

    /// <summary>
    /// Collection of keys of changed elements.
    /// </summary>
    new IReadOnlySet<TKey> ModifiedElementKeys { get; }

    /// <summary>
    /// Element key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Element equality comparer.
    /// </summary>
    IEqualityComparer<TElement> ElementComparer { get; }

    /// <summary>
    /// Returns a collection of current validation errors associated with the given element <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Collection of current validation errors associated with the given element <paramref name="key"/>.</returns>
    [Pure]
    IEnumerable GetErrors(TKey key);

    /// <summary>
    /// Returns a collection of current validation warnings associated with the given element <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Collection of current validation warnings associated with the given element <paramref name="key"/>.</returns>
    [Pure]
    IEnumerable GetWarnings(TKey key);

    /// <summary>
    /// Returns the current state of an element with the given <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Current state of an element with the given <paramref name="key"/>.</returns>
    [Pure]
    CollectionVariableElementState GetState(TKey key);
}

/// <summary>
/// Represents a generic collection of elements
/// that belong to an <see cref="IReadOnlyCollectionVariable{TKey,TElement,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface ICollectionVariableElements<TKey, TElement, TValidationResult> : ICollectionVariableElements<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Element validator that marks result as errors.
    /// </summary>
    IValidator<TElement, TValidationResult> ErrorsValidator { get; }

    /// <summary>
    /// Element validator that marks result as warnings.
    /// </summary>
    IValidator<TElement, TValidationResult> WarningsValidator { get; }

    /// <summary>
    /// Returns a collection of current validation errors associated with the given element <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Collection of current validation errors associated with the given element <paramref name="key"/>.</returns>
    [Pure]
    new Chain<TValidationResult> GetErrors(TKey key);

    /// <summary>
    /// Returns a collection of current validation warnings associated with the given element <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Collection of current validation warnings associated with the given element <paramref name="key"/>.</returns>
    [Pure]
    new Chain<TValidationResult> GetWarnings(TKey key);
}
