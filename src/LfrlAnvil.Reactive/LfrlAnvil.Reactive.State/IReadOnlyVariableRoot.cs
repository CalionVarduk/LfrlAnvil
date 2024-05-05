using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased read-only variable root node that listens to its children's events and propagates them.
/// </summary>
public interface IReadOnlyVariableRoot : IVariableNode
{
    /// <summary>
    /// Nested collection of child nodes.
    /// </summary>
    IVariableNodeCollection Nodes { get; }

    /// <summary>
    /// Event stream that emits events when variable's value changes.
    /// </summary>
    new IEventStream<IVariableRootEvent> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<IVariableRootEvent> OnValidate { get; }
}

/// <summary>
/// Represents a generic read-only variable root node that listens to its children's events and propagates them.
/// </summary>
/// <typeparam name="TKey">Child node's key type.</typeparam>
public interface IReadOnlyVariableRoot<TKey> : IReadOnlyVariableRoot
    where TKey : notnull
{
    /// <summary>
    /// Nested collection of child nodes.
    /// </summary>
    new IVariableNodeCollection<TKey> Nodes { get; }

    /// <summary>
    /// Event stream that emits events when variable's value changes.
    /// </summary>
    new IEventStream<IVariableRootEvent<TKey>> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<IVariableRootEvent<TKey>> OnValidate { get; }
}
