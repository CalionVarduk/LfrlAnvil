using System;

namespace LfrlAnvil.Computable.Automata.Exceptions;

/// <summary>
/// Represents an error related to state retrieval from a state machine.
/// </summary>
public class StateMachineStateException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="StateMachineStateException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public StateMachineStateException(string message, string paramName)
        : base( message, paramName ) { }
}
