namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlRoundFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlRoundFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode precision)
        : base( SqlFunctionType.Round, new[] { argument, precision } ) { }
}
