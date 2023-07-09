namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlToUpperFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlToUpperFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ToUpper, new[] { argument } ) { }
}
