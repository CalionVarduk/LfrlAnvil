using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql;

public class MySqlNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    protected internal MySqlNodeInterpreterFactory(MySqlNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new MySqlColumnTypeDefinitionProviderBuilder().Build() );
    }

    public MySqlNodeInterpreterOptions Options { get; }

    [Pure]
    public virtual MySqlNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new MySqlNodeInterpreter( Options, context );
    }

    [Pure]
    SqlNodeInterpreter ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext context)
    {
        return Create( context );
    }
}
