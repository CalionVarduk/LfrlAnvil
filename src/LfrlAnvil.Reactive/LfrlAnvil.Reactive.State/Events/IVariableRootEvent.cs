using System;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased value change event emitted by an <see cref="IReadOnlyVariableRoot"/>.
/// </summary>
public interface IVariableRootEvent : IVariableNodeEvent
{
    /// <summary>
    /// Child node's key type.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariableRoot Variable { get; }

    /// <summary>
    /// Key of the child node that caused this event.
    /// </summary>
    object NodeKey { get; }

    /// <summary>
    /// Source child node event.
    /// </summary>
    IVariableNodeEvent SourceEvent { get; }
}

/// <summary>
/// Represents a generic value change event emitted by an <see cref="IReadOnlyVariableRoot{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Child node's key type.</typeparam>
public interface IVariableRootEvent<TKey> : IVariableRootEvent
    where TKey : notnull
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariableRoot<TKey> Variable { get; }

    /// <summary>
    /// Key of the child node that caused this event.
    /// </summary>
    new TKey NodeKey { get; }
}
