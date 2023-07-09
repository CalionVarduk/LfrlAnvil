namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSignFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSignFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Sign, new[] { argument } ) { }
}
