using System;

namespace LfrlAnvil.Reactive.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid argument type.
/// </summary>
public class InvalidArgumentTypeException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidArgumentTypeException"/> instance.
    /// </summary>
    /// <param name="argument">Invalid argument.</param>
    /// <param name="expectedType">Expected type.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidArgumentTypeException(object? argument, Type expectedType, string paramName)
        : base( Resources.InvalidArgumentType( argument, expectedType ), paramName )
    {
        Argument = argument;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Invalid argument.
    /// </summary>
    public object? Argument { get; }

    /// <summary>
    /// Expected type.
    /// </summary>
    public Type ExpectedType { get; }
}
