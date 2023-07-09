namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSquareRootFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSquareRootFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.SquareRoot, new[] { argument } ) { }
}
