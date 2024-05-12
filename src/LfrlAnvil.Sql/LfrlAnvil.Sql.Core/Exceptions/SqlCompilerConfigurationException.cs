using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid SQL compiler configuration.
/// </summary>
public class SqlCompilerConfigurationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlCompilerConfigurationException"/> instance.
    /// </summary>
    /// <param name="errors">Collection of underlying errors.</param>
    public SqlCompilerConfigurationException(Chain<Pair<Expression, Exception>> errors)
        : base( ExceptionResources.CompilerConfigurationErrorsHaveOccurred( errors ) )
    {
        Errors = errors;
    }

    /// <summary>
    /// Collection of underlying errors.
    /// </summary>
    public Chain<Pair<Expression, Exception>> Errors { get; }
}
