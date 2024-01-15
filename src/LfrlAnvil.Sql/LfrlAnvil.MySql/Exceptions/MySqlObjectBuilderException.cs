using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.MySql.Exceptions;

public class MySqlObjectBuilderException : SqlObjectBuilderException
{
    public MySqlObjectBuilderException(string error)
        : this( Chain.Create( error ) ) { }

    public MySqlObjectBuilderException(Chain<string> errors)
        : base( MySqlDialect.Instance, errors ) { }
}
