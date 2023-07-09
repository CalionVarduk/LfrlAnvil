namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTruncateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTruncateFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Truncate, new[] { argument } ) { }
}
