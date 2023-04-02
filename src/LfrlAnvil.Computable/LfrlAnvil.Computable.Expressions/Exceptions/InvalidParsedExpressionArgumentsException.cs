using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class InvalidParsedExpressionArgumentsException : ArgumentException
{
    public InvalidParsedExpressionArgumentsException(Chain<StringSegment> argumentNames, string paramName)
        : base( Resources.InvalidExpressionArguments( argumentNames ), paramName )
    {
        ArgumentNames = argumentNames;
    }

    public Chain<StringSegment> ArgumentNames { get; }
}
