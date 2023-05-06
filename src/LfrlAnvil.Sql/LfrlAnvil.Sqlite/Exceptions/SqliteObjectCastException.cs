using System;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sqlite.Exceptions;

public class SqliteObjectCastException : SqlObjectCastException
{
    public SqliteObjectCastException(Type expected, Type actual)
        : base( SqliteDialect.Instance, expected, actual ) { }
}
