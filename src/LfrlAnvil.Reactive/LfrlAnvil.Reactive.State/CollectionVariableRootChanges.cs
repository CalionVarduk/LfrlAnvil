using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

public readonly struct CollectionVariableRootChanges<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    public static readonly CollectionVariableRootChanges<TKey, TElement> Empty = new CollectionVariableRootChanges<TKey, TElement>();

    private readonly IEnumerable<TElement>? _elementsToAdd;
    private readonly IEnumerable<TKey>? _keysToRestore;

    public CollectionVariableRootChanges(IEnumerable<TElement> elementsToAdd, IEnumerable<TKey> keysToRestore)
    {
        _elementsToAdd = elementsToAdd;
        _keysToRestore = keysToRestore;
    }

    public IEnumerable<TElement> ElementsToAdd => _elementsToAdd ?? Array.Empty<TElement>();
    public IEnumerable<TKey> KeysToRestore => _keysToRestore ?? Array.Empty<TKey>();
}
