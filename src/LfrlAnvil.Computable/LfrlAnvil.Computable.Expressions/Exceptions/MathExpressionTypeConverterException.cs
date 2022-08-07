using System;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class MathExpressionTypeConverterException : InvalidOperationException
{
    public MathExpressionTypeConverterException(string message, MathExpressionTypeConverter converter)
        : base( message )
    {
        Converter = converter;
    }

    public MathExpressionTypeConverter Converter { get; }
}
