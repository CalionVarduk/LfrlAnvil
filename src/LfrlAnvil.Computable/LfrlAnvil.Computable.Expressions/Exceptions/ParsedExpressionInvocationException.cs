using System;
using System.Collections.Generic;
using System.Globalization;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionInvocationException : Exception
{
    public ParsedExpressionInvocationException(string? format, params object?[] args)
        : base( string.Format( CultureInfo.InvariantCulture, format ?? string.Empty, args ) )
    {
        Format = format ?? string.Empty;
        Args = args;
    }

    public string Format { get; }
    public IReadOnlyList<object?> Args { get; }
}
