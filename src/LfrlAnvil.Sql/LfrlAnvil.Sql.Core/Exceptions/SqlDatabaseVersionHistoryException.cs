using System;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlDatabaseVersionHistoryException : InvalidOperationException
{
    public SqlDatabaseVersionHistoryException(Chain<string> errors)
        : base( ExceptionResources.GetVersionHistoryErrors( errors ) ) { }
}
