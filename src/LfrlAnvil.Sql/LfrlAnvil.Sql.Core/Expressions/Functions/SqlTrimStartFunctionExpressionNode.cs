namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTrimStartFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTrimStartFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? characters)
        : base( SqlFunctionType.TrimStart, characters is null ? new[] { argument } : new[] { argument, characters } ) { }
}
