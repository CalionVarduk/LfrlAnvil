using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to invalid expression argument names.
/// </summary>
public class InvalidParsedExpressionArgumentsException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidParsedExpressionArgumentsException"/> instance.
    /// </summary>
    /// <param name="argumentNames">Invalid argument names.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidParsedExpressionArgumentsException(Chain<StringSegment> argumentNames, string paramName)
        : base( Resources.InvalidExpressionArguments( argumentNames ), paramName )
    {
        ArgumentNames = argumentNames;
    }

    /// <summary>
    /// Invalid argument names.
    /// </summary>
    public Chain<StringSegment> ArgumentNames { get; }
}
