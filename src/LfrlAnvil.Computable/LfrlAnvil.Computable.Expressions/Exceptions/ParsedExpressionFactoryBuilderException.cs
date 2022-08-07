using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionFactoryBuilderException : InvalidOperationException
{
    public ParsedExpressionFactoryBuilderException(Chain<string> messages)
        : base( Resources.FailedExpressionFactoryCreation( messages ) )
    {
        Messages = messages;
    }

    public Chain<string> Messages { get; }
}
