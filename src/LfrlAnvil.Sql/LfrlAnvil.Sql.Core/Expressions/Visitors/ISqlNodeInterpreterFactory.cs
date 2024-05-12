using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a factory of SQL node interpreters.
/// </summary>
public interface ISqlNodeInterpreterFactory
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreter"/> instance.
    /// </summary>
    /// <param name="context">Underlying context.</param>
    /// <returns>New <see cref="SqlNodeInterpreter"/> instance.</returns>
    [Pure]
    SqlNodeInterpreter Create(SqlNodeInterpreterContext context);
}
