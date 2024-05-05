using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased variable change event emitted by an <see cref="IReadOnlyCollectionVariableRoot"/>.
/// </summary>
public interface ICollectionVariableRootChangeEvent : IVariableNodeEvent
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
    new IReadOnlyCollectionVariableRoot Variable { get; }

    /// <summary>
    /// Collection of elements added due to this event.
    /// </summary>
    IReadOnlyList<IVariableNode> AddedElements { get; }

    /// <summary>
    /// Collection of elements removed due to this event.
    /// </summary>
    IReadOnlyList<IVariableNode> RemovedElements { get; }

    /// <summary>
    /// Collection of elements restored due to this event.
    /// </summary>
    IReadOnlyList<IVariableNode> RestoredElements { get; }

    /// <summary>
    /// Specifies the source of this value change.
    /// </summary>
    VariableChangeSource Source { get; }

    /// <summary>
    /// Source child node event.
    /// </summary>
    IVariableNodeEvent? SourceEvent { get; }
}

/// <summary>
/// Represents a generic variable change event emitted by an <see cref="IReadOnlyCollectionVariableRoot{TKey,TElement}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public interface ICollectionVariableRootChangeEvent<TKey, TElement> : ICollectionVariableRootChangeEvent
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyCollectionVariableRoot<TKey, TElement> Variable { get; }

    /// <summary>
    /// Collection of elements added due to this event.
    /// </summary>
    new IReadOnlyList<TElement> AddedElements { get; }

    /// <summary>
    /// Collection of elements removed due to this event.
    /// </summary>
    new IReadOnlyList<TElement> RemovedElements { get; }

    /// <summary>
    /// Collection of elements restored due to this event.
    /// </summary>
    new IReadOnlyList<TElement> RestoredElements { get; }
}
