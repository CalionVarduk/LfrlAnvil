using System;

namespace LfrlAnvil.Computable.Automata.Exceptions;

/// <summary>
/// Represents an error related to transition retrieval from a state machine.
/// </summary>
public class StateMachineTransitionException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="StateMachineTransitionException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public StateMachineTransitionException(string message, string paramName)
        : base( message, paramName ) { }
}
