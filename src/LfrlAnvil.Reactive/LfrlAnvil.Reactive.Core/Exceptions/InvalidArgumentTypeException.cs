using System;

namespace LfrlAnvil.Reactive.Exceptions;

public class InvalidArgumentTypeException : ArgumentException
{
    public InvalidArgumentTypeException(object? argument, Type expectedType, string paramName)
        : base( Resources.InvalidArgumentType( argument, expectedType ), paramName )
    {
        Argument = argument;
        ExpectedType = expectedType;
    }

    public object? Argument { get; }
    public Type ExpectedType { get; }
}
