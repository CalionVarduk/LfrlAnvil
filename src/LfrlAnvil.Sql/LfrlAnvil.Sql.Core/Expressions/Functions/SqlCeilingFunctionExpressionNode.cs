namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCeilingFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCeilingFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Ceiling, new[] { argument } ) { }
}
