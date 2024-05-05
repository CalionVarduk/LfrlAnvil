using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents changes to make to <see cref="ICollectionVariableRoot{TKey,TElement,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public readonly struct CollectionVariableRootChanges<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Represents an empty collection.
    /// </summary>
    public static readonly CollectionVariableRootChanges<TKey, TElement> Empty = new CollectionVariableRootChanges<TKey, TElement>();

    private readonly IEnumerable<TElement>? _elementsToAdd;
    private readonly IEnumerable<TKey>? _keysToRestore;

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRootChanges{TKey,TElement}"/> instance.
    /// </summary>
    /// <param name="elementsToAdd">Collection of elements to add.</param>
    /// <param name="keysToRestore">Collection of keys of removed elements to restore.</param>
    public CollectionVariableRootChanges(IEnumerable<TElement> elementsToAdd, IEnumerable<TKey> keysToRestore)
    {
        _elementsToAdd = elementsToAdd;
        _keysToRestore = keysToRestore;
    }

    /// <summary>
    /// Collection of elements to add.
    /// </summary>
    public IEnumerable<TElement> ElementsToAdd => _elementsToAdd ?? Array.Empty<TElement>();

    /// <summary>
    /// Collection of keys of removed elements to restore.
    /// </summary>
    public IEnumerable<TKey> KeysToRestore => _keysToRestore ?? Array.Empty<TKey>();
}
