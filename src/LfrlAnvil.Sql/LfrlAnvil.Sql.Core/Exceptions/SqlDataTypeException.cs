using System;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlDataTypeException : InvalidOperationException
{
    public SqlDataTypeException(Chain<Pair<SqlDataTypeParameter, int>> parameters)
        : base( ExceptionResources.InvalidDataTypeParameters( parameters ) ) { }
}
