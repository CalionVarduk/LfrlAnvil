namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLastIndexOfFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlLastIndexOfFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value)
        : base( SqlFunctionType.LastIndexOf, new[] { argument, value } ) { }
}
