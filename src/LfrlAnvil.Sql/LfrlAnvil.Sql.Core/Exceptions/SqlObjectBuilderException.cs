using System;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid <see cref="ISqlObjectBuilder"/> state.
/// </summary>
public class SqlObjectBuilderException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect with which the object is associated.</param>
    /// <param name="errors">Collection of error messages.</param>
    public SqlObjectBuilderException(SqlDialect dialect, Chain<string> errors)
        : base( ExceptionResources.GetObjectBuilderErrors( dialect, errors ) )
    {
        Dialect = dialect;
        Errors = errors;
    }

    /// <summary>
    /// SQL dialect with which the object is associated.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Collection of error messages.
    /// </summary>
    public Chain<string> Errors { get; }
}
