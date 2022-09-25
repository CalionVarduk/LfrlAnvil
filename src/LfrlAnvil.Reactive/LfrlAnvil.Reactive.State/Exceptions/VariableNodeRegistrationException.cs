using System;

namespace LfrlAnvil.Reactive.State.Exceptions;

public class VariableNodeRegistrationException : InvalidOperationException
{
    public VariableNodeRegistrationException(string message, IVariableNode parent, IVariableNode child)
        : base( message )
    {
        Parent = parent;
        Child = child;
    }

    public IVariableNode Parent { get; }
    public IVariableNode Child { get; }
}
