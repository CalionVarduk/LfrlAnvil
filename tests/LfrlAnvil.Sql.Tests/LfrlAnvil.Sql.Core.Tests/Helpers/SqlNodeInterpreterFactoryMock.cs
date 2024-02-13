using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlNodeInterpreterFactoryMock : ISqlNodeInterpreterFactory
{
    [Pure]
    public SqlNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new SqlNodeDebugInterpreter( context );
    }
}
