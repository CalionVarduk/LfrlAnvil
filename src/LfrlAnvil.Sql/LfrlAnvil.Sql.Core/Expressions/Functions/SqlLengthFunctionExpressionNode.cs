namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLengthFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlLengthFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Length, new[] { argument } ) { }
}
