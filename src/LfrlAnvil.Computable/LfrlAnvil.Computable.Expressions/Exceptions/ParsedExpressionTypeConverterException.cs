using System;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid type cast.
/// </summary>
public class ParsedExpressionTypeConverterException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeConverterException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="converter">Used type converter construct.</param>
    public ParsedExpressionTypeConverterException(string message, ParsedExpressionTypeConverter converter)
        : base( message )
    {
        Converter = converter;
    }

    /// <summary>
    /// Used type converter construct.
    /// </summary>
    public ParsedExpressionTypeConverter Converter { get; }
}
