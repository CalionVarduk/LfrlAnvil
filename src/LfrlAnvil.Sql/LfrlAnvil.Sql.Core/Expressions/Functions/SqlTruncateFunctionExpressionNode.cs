namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTruncateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTruncateFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? precision)
        : base( SqlFunctionType.Truncate, precision is null ? new[] { argument } : new[] { argument, precision } ) { }
}
