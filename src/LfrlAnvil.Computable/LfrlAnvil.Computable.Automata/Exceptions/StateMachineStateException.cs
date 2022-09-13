using System;

namespace LfrlAnvil.Computable.Automata.Exceptions;

public class StateMachineStateException : ArgumentException
{
    public StateMachineStateException(string message, string paramName)
        : base( message, paramName ) { }
}
