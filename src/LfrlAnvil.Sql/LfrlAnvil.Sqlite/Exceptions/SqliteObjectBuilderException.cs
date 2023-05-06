using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sqlite.Exceptions;

public class SqliteObjectBuilderException : SqlObjectBuilderException
{
    public SqliteObjectBuilderException(string error)
        : this( Chain.Create( error ) ) { }

    public SqliteObjectBuilderException(Chain<string> errors)
        : base( SqliteDialect.Instance, errors ) { }
}
