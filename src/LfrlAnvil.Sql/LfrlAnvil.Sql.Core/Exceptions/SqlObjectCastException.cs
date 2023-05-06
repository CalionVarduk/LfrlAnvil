using System;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlObjectCastException : InvalidCastException
{
    public SqlObjectCastException(SqlDialect dialect, Type expected, Type actual)
        : base( ExceptionResources.GetObjectCastMessage( dialect, expected, actual ) )
    {
        Dialect = dialect;
        Expected = expected;
        Actual = actual;
    }

    public SqlDialect Dialect { get; }
    public Type Expected { get; }
    public Type Actual { get; }
}
