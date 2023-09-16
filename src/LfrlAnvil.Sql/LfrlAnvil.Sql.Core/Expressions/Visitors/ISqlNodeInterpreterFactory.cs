using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public interface ISqlNodeInterpreterFactory
{
    [Pure]
    SqlNodeInterpreter Create(SqlNodeInterpreterContext context);
}
