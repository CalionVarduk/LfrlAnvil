using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql;

/// <inheritdoc cref="ISqlNodeInterpreterFactory" />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public class MySqlNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterFactory"/> instance.
    /// </summary>
    /// <param name="options"><see cref="MySqlNodeInterpreterOptions"/> instance applied to created node interpreters.</param>
    protected internal MySqlNodeInterpreterFactory(MySqlNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new MySqlColumnTypeDefinitionProviderBuilder().Build() );
    }

    /// <summary>
    /// <see cref="MySqlNodeInterpreterOptions"/> instance applied to created node interpreters.
    /// </summary>
    public MySqlNodeInterpreterOptions Options { get; }

    /// <inheritdoc cref="ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext)" />
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
