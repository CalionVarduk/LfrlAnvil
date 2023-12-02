using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlCompilerConfigurationException : InvalidOperationException
{
    public SqlCompilerConfigurationException(Chain<Pair<Expression, Exception>> errors)
        : base( ExceptionResources.CompilerConfigurationErrorsHaveOccurred( errors ) )
    {
        Errors = errors;
    }

    public Chain<Pair<Expression, Exception>> Errors { get; }
}
