namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlIndexOfFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlIndexOfFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value)
        : base( SqlFunctionType.IndexOf, new[] { argument, value } ) { }
}
