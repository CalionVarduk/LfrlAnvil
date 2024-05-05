using System;

namespace LfrlAnvil.Reactive.State.Exceptions;

/// <summary>
/// Represents an error that occurred during child <see cref="IVariableNode"/> registration.
/// </summary>
public class VariableNodeRegistrationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="VariableNodeRegistrationException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="parent">Parent node.</param>
    /// <param name="child">Child node.</param>
    public VariableNodeRegistrationException(string message, IVariableNode parent, IVariableNode child)
        : base( message )
    {
        Parent = parent;
        Child = child;
    }

    /// <summary>
    /// Parent node.
    /// </summary>
    public IVariableNode Parent { get; }

    /// <summary>
    /// Child node.
    /// </summary>
    public IVariableNode Child { get; }
}
