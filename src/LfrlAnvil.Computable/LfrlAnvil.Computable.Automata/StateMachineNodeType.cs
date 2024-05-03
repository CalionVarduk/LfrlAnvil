using System;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents the type of state machine's node.
/// </summary>
[Flags]
public enum StateMachineNodeType : byte
{
    /// <summary>
    /// Specifies that the node is a standard node.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Specifies that the node is an initial node, at which the state machine starts.
    /// </summary>
    Initial = 1,

    /// <summary>
    /// Specifies that the node is marked as accept or final node.
    /// </summary>
    Accept = 2,

    /// <summary>
    /// Specifies that no <see cref="Accept"/> node can be reached from this node.
    /// </summary>
    Dead = 4
}
