using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class FunctionNodeMock : SqlFunctionExpressionNode
{
    public FunctionNodeMock(SqlExpressionNode[]? arguments = null)
        : base( arguments ?? Array.Empty<SqlExpressionNode>() ) { }
}
