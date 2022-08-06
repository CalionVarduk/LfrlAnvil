using System;
using LfrlAnvil.Mathematical.Expressions.Errors;

namespace LfrlAnvil.Mathematical.Expressions.Exceptions;

public class MathExpressionCreationException : InvalidOperationException
{
    public MathExpressionCreationException(string input, Chain<MathExpressionBuilderError> errors)
        : base( Resources.FailedExpressionCreation( input, errors ) )
    {
        Input = input;
        Errors = errors;
    }

    public string Input { get; }
    public Chain<MathExpressionBuilderError> Errors { get; }
}
