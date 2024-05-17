using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public class PostgreSqlNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    /// <summary>
    /// Creates a new <see cref="PostgreSqlNodeInterpreterFactory"/> instance.
    /// </summary>
    /// <param name="options"><see cref="PostgreSqlNodeInterpreterOptions"/> instance applied to created node interpreters.</param>
    protected internal PostgreSqlNodeInterpreterFactory(PostgreSqlNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new PostgreSqlColumnTypeDefinitionProviderBuilder().Build() );
    }

    /// <summary>
    /// <see cref="PostgreSqlNodeInterpreterOptions"/> instance applied to created node interpreters.
    /// </summary>
    public PostgreSqlNodeInterpreterOptions Options { get; }

    /// <inheritdoc cref="ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext)" />
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
