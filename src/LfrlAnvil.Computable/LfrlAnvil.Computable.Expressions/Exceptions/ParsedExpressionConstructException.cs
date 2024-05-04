using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error associated with an expression construct.
/// </summary>
public class ParsedExpressionConstructException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstructException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="constructType">Construct's type.</param>
    public ParsedExpressionConstructException(string message, Type constructType)
        : base( message )
    {
        ConstructType = constructType;
    }

    /// <summary>
    /// Construct's type.
    /// </summary>
    public Type ConstructType { get; }
}
