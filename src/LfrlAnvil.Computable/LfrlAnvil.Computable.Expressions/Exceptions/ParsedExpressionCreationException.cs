using System;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionCreationException : InvalidOperationException
{
    public ParsedExpressionCreationException(string input, Chain<ParsedExpressionBuilderError> errors)
        : base( Resources.FailedExpressionCreation( input, errors ) )
    {
        Input = input;
        Errors = errors;
    }

    public string Input { get; }
    public Chain<ParsedExpressionBuilderError> Errors { get; }
}
