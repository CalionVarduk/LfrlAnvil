using System;
using LfrlAnvil.Mathematical.Expressions.Constructs;

namespace LfrlAnvil.Mathematical.Expressions.Exceptions;

public class MathExpressionTypeConverterException : InvalidOperationException
{
    public MathExpressionTypeConverterException(string message, MathExpressionTypeConverter converter)
        : base( message )
    {
        Converter = converter;
    }

    public MathExpressionTypeConverter Converter { get; }
}
