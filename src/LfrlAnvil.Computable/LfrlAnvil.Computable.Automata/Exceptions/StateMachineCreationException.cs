using System;

namespace LfrlAnvil.Computable.Automata.Exceptions;

/// <summary>
/// Represents an error that occurred during state machine creation.
/// </summary>
public class StateMachineCreationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="StateMachineCreationException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    public StateMachineCreationException(string message)
        : base( message ) { }
}
