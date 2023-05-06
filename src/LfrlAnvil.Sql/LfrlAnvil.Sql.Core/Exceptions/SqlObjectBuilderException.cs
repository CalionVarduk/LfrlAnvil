using System;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlObjectBuilderException : InvalidOperationException
{
    public SqlObjectBuilderException(SqlDialect dialect, Chain<string> errors)
        : base( ExceptionResources.GetObjectBuilderErrors( dialect, errors ) )
    {
        Dialect = dialect;
        Errors = errors;
    }

    public SqlDialect Dialect { get; }
    public Chain<string> Errors { get; }
}
