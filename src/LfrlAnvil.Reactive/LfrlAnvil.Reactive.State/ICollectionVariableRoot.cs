using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a generic collection variable of <see cref="VariableNode"/> elements
/// that listens to its children's events and propagates them.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface ICollectionVariableRoot<TKey, TElement, TValidationResult>
    : IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Changes the <see cref="IReadOnlyCollectionVariableRoot{TKey,TElement}.Elements"/>.
    /// </summary>
    /// <param name="changes">Changes to make to <see cref="IReadOnlyCollectionVariableRoot{TKey,TElement}.Elements"/>.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Change(CollectionVariableRootChanges<TKey, TElement> changes);

    /// <summary>
    /// Adds an element to this collection.
    /// </summary>
    /// <param name="element">Element to add.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Add(TElement element);

    /// <summary>
    /// Adds a collection of elements to this collection.
    /// </summary>
    /// <param name="elements">Collection of elements to add.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Add(IEnumerable<TElement> elements);

    /// <summary>
    /// Restores a removed element associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to restore.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Restore(TKey key);

    /// <summary>
    /// Restores removed elements associated with the provided collection of <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Collection of keys to restore.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Restore(IEnumerable<TKey> keys);

    /// <summary>
    /// Removes an element associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Remove(TKey key);

    /// <summary>
    /// Removes elements associated with the provided collection of <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Collection of keys to remove.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Remove(IEnumerable<TKey> keys);

    /// <summary>
    /// Removes all elements from this collection.
    /// </summary>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Clear();

    /// <summary>
    /// Refreshes this variable.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Refreshes this variable's validation.
    /// </summary>
    void RefreshValidation();

    /// <summary>
    /// Removes all errors and warnings from this variable.
    /// </summary>
    void ClearValidation();
}
