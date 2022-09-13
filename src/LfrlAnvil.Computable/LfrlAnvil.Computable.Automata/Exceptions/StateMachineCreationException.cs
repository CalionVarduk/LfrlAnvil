using System;

namespace LfrlAnvil.Computable.Automata.Exceptions;

public class StateMachineCreationException : InvalidOperationException
{
    public StateMachineCreationException(string message)
        : base( message ) { }
}
