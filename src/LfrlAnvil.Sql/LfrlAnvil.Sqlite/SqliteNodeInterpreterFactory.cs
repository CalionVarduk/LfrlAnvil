using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sqlite;

public class SqliteNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    private readonly SqliteColumnTypeDefinitionProvider _columnTypeDefinitions;

    protected internal SqliteNodeInterpreterFactory(SqliteColumnTypeDefinitionProvider columnTypeDefinitions)
    {
        _columnTypeDefinitions = columnTypeDefinitions;
    }

    [Pure]
    public virtual SqliteNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new SqliteNodeInterpreter( _columnTypeDefinitions, context );
    }

    [Pure]
    SqlNodeInterpreter ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext context)
    {
        return Create( context );
    }
}
