using System;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred during preparation of an SQL expression.
/// </summary>
public class SqlCompilerException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlCompilerException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect.</param>
    /// <param name="error">Error message.</param>
    public SqlCompilerException(SqlDialect dialect, string error)
        : this( dialect, Chain.Create( error ) ) { }

    /// <summary>
    /// Creates a new <see cref="SqlCompilerException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect.</param>
    /// <param name="errors">Collection of error messages.</param>
    public SqlCompilerException(SqlDialect dialect, Chain<string> errors)
        : base( ExceptionResources.CompilerErrorsHaveOccurred( dialect, errors ) )
    {
        Dialect = dialect;
    }

    /// <summary>
    /// SQL dialect.
    /// </summary>
    public SqlDialect Dialect { get; }
}
