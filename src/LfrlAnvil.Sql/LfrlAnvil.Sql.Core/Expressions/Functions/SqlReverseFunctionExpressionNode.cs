namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlReverseFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlReverseFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Reverse, new[] { argument } ) { }
}
