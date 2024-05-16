using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public class SqliteNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterFactory"/> instance.
    /// </summary>
    /// <param name="options"><see cref="SqliteNodeInterpreterOptions"/> instance applied to created node interpreters.</param>
    protected internal SqliteNodeInterpreterFactory(SqliteNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new SqliteColumnTypeDefinitionProviderBuilder().Build() );
    }

    /// <summary>
    /// <see cref="SqliteNodeInterpreterOptions"/> instance applied to created node interpreters.
    /// </summary>
    public SqliteNodeInterpreterOptions Options { get; }

    /// <inheritdoc cref="ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext)" />
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
