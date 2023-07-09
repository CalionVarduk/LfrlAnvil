namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTrimFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTrimFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? characters)
        : base( SqlFunctionType.Trim, characters is null ? new[] { argument } : new[] { argument, characters } ) { }
}
