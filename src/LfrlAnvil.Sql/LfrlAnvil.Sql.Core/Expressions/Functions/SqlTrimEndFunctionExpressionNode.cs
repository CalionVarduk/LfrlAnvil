namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTrimEndFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTrimEndFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? characters)
        : base( SqlFunctionType.TrimEnd, characters is null ? new[] { argument } : new[] { argument, characters } ) { }
}
