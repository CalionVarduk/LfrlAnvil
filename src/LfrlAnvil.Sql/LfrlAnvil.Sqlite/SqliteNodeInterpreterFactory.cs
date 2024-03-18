using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sqlite;

public class SqliteNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    protected internal SqliteNodeInterpreterFactory(SqliteNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new SqliteColumnTypeDefinitionProviderBuilder().Build() );
    }

    public SqliteNodeInterpreterOptions Options { get; }

    [Pure]
    public virtual SqliteNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new SqliteNodeInterpreter( Options, context );
    }

    [Pure]
    SqlNodeInterpreter ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext context)
    {
        return Create( context );
    }
}
