using System;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionTypeConverterException : InvalidOperationException
{
    public ParsedExpressionTypeConverterException(string message, ParsedExpressionTypeConverter converter)
        : base( message )
    {
        Converter = converter;
    }

    public ParsedExpressionTypeConverter Converter { get; }
}
