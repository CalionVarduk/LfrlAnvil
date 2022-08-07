using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class MathExpressionFactoryBuilderException : InvalidOperationException
{
    public MathExpressionFactoryBuilderException(Chain<string> messages)
        : base( Resources.FailedExpressionFactoryCreation( messages ) )
    {
        Messages = messages;
    }

    public Chain<string> Messages { get; }
}
