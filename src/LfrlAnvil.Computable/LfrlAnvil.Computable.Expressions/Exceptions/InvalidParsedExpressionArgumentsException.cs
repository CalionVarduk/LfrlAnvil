using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class InvalidParsedExpressionArgumentsException : ArgumentException
{
    public InvalidParsedExpressionArgumentsException(Chain<StringSlice> argumentNames, string paramName)
        : base( Resources.InvalidExpressionArguments( argumentNames ), paramName )
    {
        ArgumentNames = argumentNames;
    }

    public Chain<StringSlice> ArgumentNames { get; }
}
