using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlFunctionNodeMock : SqlFunctionExpressionNode
{
    public SqlFunctionNodeMock(SqlExpressionNode[]? arguments = null)
        : base( arguments ?? Array.Empty<SqlExpressionNode>() ) { }
}
