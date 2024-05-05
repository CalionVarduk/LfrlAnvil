using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased variable state node.
/// </summary>
public interface IVariableNode
{
    /// <summary>
    /// Parent node.
    /// </summary>
    IVariableNode? Parent { get; }

    /// <summary>
    /// Specifies this node's current state.
    /// </summary>
    VariableState State { get; }

    /// <summary>
    /// Event stream that emits events when variable's value changes.
    /// </summary>
    IEventStream<IVariableNodeEvent> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    IEventStream<IVariableNodeEvent> OnValidate { get; }

    /// <summary>
    /// Returns the collection of all children nodes.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<IVariableNode> GetChildren();
}
