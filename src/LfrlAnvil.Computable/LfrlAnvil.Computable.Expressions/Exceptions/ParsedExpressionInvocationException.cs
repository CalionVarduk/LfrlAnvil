using System;
using System.Collections.Generic;
using System.Globalization;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that can be thrown by compiled expression delegates.
/// </summary>
public class ParsedExpressionInvocationException : Exception
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionInvocationException"/> instance.
    /// </summary>
    /// <param name="format">Exception's message format.</param>
    /// <param name="args">Exception's message arguments.</param>
    public ParsedExpressionInvocationException(string? format, params object?[] args)
        : base( string.Format( CultureInfo.InvariantCulture, format ?? string.Empty, args ) )
    {
        Format = format ?? string.Empty;
        Args = args;
    }

    /// <summary>
    /// Exception's message format.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Exception's message arguments.
    /// </summary>
    public IReadOnlyList<object?> Args { get; }
}
