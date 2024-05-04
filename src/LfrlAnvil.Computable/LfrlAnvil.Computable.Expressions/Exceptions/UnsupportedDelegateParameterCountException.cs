using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to a number of nested delegate parameters with a closure being too large.
/// </summary>
public class UnsupportedDelegateParameterCountException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="UnsupportedDelegateParameterCountException"/> instance.
    /// </summary>
    /// <param name="parameterCount">Parameter count.</param>
    public UnsupportedDelegateParameterCountException(int parameterCount)
        : base( Resources.UnsupportedDelegateParameterCount( parameterCount ) )
    {
        ParameterCount = parameterCount;
    }

    /// <summary>
    /// Parameter count.
    /// </summary>
    public int ParameterCount { get; }
}
