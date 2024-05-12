using System;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid DB version history.
/// </summary>
public class SqlDatabaseVersionHistoryException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersionHistoryException"/> instance.
    /// </summary>
    /// <param name="errors">Collection of error messages.</param>
    public SqlDatabaseVersionHistoryException(Chain<string> errors)
        : base( ExceptionResources.GetVersionHistoryErrors( errors ) ) { }
}
