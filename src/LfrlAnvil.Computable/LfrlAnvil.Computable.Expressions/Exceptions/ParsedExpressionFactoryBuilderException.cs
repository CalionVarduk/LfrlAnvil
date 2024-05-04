using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred during an attempt to create an <see cref="IParsedExpressionFactory"/> instance.
/// </summary>
public class ParsedExpressionFactoryBuilderException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFactoryBuilderException"/> instance.
    /// </summary>
    /// <param name="messages">Collection of error messages.</param>
    public ParsedExpressionFactoryBuilderException(Chain<string> messages)
        : base( Resources.FailedExpressionFactoryCreation( messages ) )
    {
        Messages = messages;
    }

    /// <summary>
    /// Collection of error messages.
    /// </summary>
    public Chain<string> Messages { get; }
}
