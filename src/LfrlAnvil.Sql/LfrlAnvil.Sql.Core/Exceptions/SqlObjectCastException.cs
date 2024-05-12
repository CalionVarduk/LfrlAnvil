using System;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an SQL object of invalid type.
/// </summary>
public class SqlObjectCastException : InvalidCastException
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectCastException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect with which the object is associated.</param>
    /// <param name="expected">Expected object type.</param>
    /// <param name="actual">Actual object type.</param>
    public SqlObjectCastException(SqlDialect dialect, Type expected, Type actual)
        : base( ExceptionResources.GetObjectCastMessage( dialect, expected, actual ) )
    {
        Dialect = dialect;
        Expected = expected;
        Actual = actual;
    }

    /// <summary>
    /// SQL dialect with which the object is associated.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Expected object type.
    /// </summary>
    public Type Expected { get; }

    /// <summary>
    /// Actual object type.
    /// </summary>
    public Type Actual { get; }
}
