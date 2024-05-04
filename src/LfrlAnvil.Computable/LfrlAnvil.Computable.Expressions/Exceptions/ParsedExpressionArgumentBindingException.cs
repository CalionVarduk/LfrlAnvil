using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid argument name during argument binding.
/// </summary>
public class ParsedExpressionArgumentBindingException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionArgumentBindingException"/> instance.
    /// </summary>
    public ParsedExpressionArgumentBindingException()
        : base( Resources.CannotBindValueToArgumentThatDoesNotExist ) { }
}
