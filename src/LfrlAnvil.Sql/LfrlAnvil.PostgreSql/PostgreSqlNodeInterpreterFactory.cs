using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.PostgreSql;

public class PostgreSqlNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    protected internal PostgreSqlNodeInterpreterFactory(PostgreSqlNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new PostgreSqlColumnTypeDefinitionProviderBuilder().Build() );
    }

    public PostgreSqlNodeInterpreterOptions Options { get; }

    [Pure]
    public virtual PostgreSqlNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new PostgreSqlNodeInterpreter( Options, context );
    }

    [Pure]
    SqlNodeInterpreter ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext context)
    {
        return Create( context );
    }
}
