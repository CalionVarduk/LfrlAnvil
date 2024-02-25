using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlNodeInterpreterFactoryMock : ISqlNodeInterpreterFactory
{
    [Pure]
    public SqlNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new SqlNodeDebugInterpreter( context );
    }
}
