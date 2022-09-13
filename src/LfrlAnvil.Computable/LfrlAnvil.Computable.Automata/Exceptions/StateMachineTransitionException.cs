using System;

namespace LfrlAnvil.Computable.Automata.Exceptions;

public class StateMachineTransitionException : ArgumentException
{
    public StateMachineTransitionException(string message, string paramName)
        : base( message, paramName ) { }
}
