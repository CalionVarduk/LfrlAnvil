using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class InvalidParsedExpressionArgumentsException : ArgumentException
{
    public InvalidParsedExpressionArgumentsException(Chain<ReadOnlyMemory<char>> argumentNames, string paramName)
        : base( Resources.InvalidExpressionArguments( argumentNames ), paramName )
    {
        ArgumentNames = argumentNames;
    }

    public Chain<ReadOnlyMemory<char>> ArgumentNames { get; }
}
