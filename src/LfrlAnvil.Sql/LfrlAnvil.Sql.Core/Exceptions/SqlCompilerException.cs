using System;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlCompilerException : InvalidOperationException
{
    public SqlCompilerException(SqlDialect dialect, string error)
        : this( dialect, Chain.Create( error ) ) { }

    public SqlCompilerException(SqlDialect dialect, Chain<string> errors)
        : base( ExceptionResources.CompilerErrorsHaveOccurred( dialect, errors ) )
    {
        Dialect = dialect;
    }

    public SqlDialect Dialect { get; }
}
