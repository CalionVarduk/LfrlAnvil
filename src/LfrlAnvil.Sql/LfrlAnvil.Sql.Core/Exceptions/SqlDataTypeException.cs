using System;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to invalid <see cref="ISqlDataType"/> parameter values.
/// </summary>
public class SqlDataTypeException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlDataTypeException"/> instance.
    /// </summary>
    /// <param name="parameters">Collection of (parameter-definition, invalid-value) pairs.</param>
    public SqlDataTypeException(Chain<Pair<SqlDataTypeParameter, int>> parameters)
        : base( ExceptionResources.InvalidDataTypeParameters( parameters ) ) { }
}
