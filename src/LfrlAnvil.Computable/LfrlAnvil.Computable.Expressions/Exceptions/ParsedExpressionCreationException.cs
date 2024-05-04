using System;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred during an attempt to create an <see cref="IParsedExpression{TArg,TResult}"/> instance.
/// </summary>
public class ParsedExpressionCreationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCreationException"/> instance.
    /// </summary>
    /// <param name="input">Parsed input.</param>
    /// <param name="errors">Collection of underlying errors.</param>
    public ParsedExpressionCreationException(string input, Chain<ParsedExpressionBuilderError> errors)
        : base( Resources.FailedExpressionCreation( input, errors ) )
    {
        Input = input;
        Errors = errors;
    }

    /// <summary>
    /// Parsed input.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// Collection of underlying errors.
    /// </summary>
    public Chain<ParsedExpressionBuilderError> Errors { get; }
}
