using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased variable change event emitted by an <see cref="IReadOnlyCollectionVariable"/>.
/// </summary>
public interface ICollectionVariableChangeEvent : IVariableNodeEvent
{
    /// <summary>
    /// Key type.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Element type.
    /// </summary>
    Type ElementType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyCollectionVariable Variable { get; }

    /// <summary>
    /// Collection of elements added due to this event.
    /// </summary>
    IReadOnlyList<ICollectionVariableElementSnapshot> AddedElements { get; }

    /// <summary>
    /// Collection of elements removed due to this event.
    /// </summary>
    IReadOnlyList<ICollectionVariableElementSnapshot> RemovedElements { get; }

    /// <summary>
    /// Collection of elements refreshed due to this event.
    /// </summary>
    IReadOnlyList<ICollectionVariableElementSnapshot> RefreshedElements { get; }

    /// <summary>
    /// Collection of elements replaced due to this event.
    /// </summary>
    IReadOnlyList<ICollectionVariableElementSnapshot> ReplacedElements { get; }

    /// <summary>
    /// Specifies the source of this value change.
    /// </summary>
    VariableChangeSource Source { get; }
}

/// <summary>
/// Represents a generic variable change event emitted by an <see cref="IReadOnlyCollectionVariable{TKey,TElement}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public interface ICollectionVariableChangeEvent<TKey, TElement> : ICollectionVariableChangeEvent
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyCollectionVariable<TKey, TElement> Variable { get; }

    /// <summary>
    /// Collection of elements added due to this event.
    /// </summary>
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> AddedElements { get; }

    /// <summary>
    /// Collection of elements removed due to this event.
    /// </summary>
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> RemovedElements { get; }

    /// <summary>
    /// Collection of elements refreshed due to this event.
    /// </summary>
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> RefreshedElements { get; }

    /// <summary>
    /// Collection of elements replaced due to this event.
    /// </summary>
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> ReplacedElements { get; }
}
