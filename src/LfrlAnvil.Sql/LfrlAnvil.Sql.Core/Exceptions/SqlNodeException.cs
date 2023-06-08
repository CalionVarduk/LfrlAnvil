using System;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlNodeException : InvalidOperationException
{
    public SqlNodeException(string message)
        : base( message ) { }
}
