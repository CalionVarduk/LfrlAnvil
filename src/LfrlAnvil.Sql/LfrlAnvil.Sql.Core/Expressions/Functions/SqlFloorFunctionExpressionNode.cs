namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlFloorFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlFloorFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Floor, new[] { argument } ) { }
}
