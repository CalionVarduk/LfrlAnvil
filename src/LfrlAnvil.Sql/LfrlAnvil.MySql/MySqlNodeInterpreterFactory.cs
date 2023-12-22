using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql;

public class MySqlNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    private readonly MySqlColumnTypeDefinitionProvider _columnTypeDefinitions;

    protected internal MySqlNodeInterpreterFactory(MySqlColumnTypeDefinitionProvider columnTypeDefinitions)
    {
        _columnTypeDefinitions = columnTypeDefinitions;
    }

    [Pure]
    public virtual MySqlNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new MySqlNodeInterpreter( _columnTypeDefinitions, context );
    }

    [Pure]
    SqlNodeInterpreter ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext context)
    {
        return Create( context );
    }
}
