using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class InvalidMathExpressionArgumentsException : ArgumentException
{
    public InvalidMathExpressionArgumentsException(Chain<ReadOnlyMemory<char>> argumentNames, string paramName)
        : base( Resources.InvalidExpressionArguments( argumentNames ), paramName )
    {
        ArgumentNames = argumentNames;
    }

    public Chain<ReadOnlyMemory<char>> ArgumentNames { get; }
}
