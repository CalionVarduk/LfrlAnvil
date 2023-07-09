namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSubstringFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSubstringFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode startIndex, SqlExpressionNode? length)
        : base( SqlFunctionType.Substring, length is null ? new[] { argument, startIndex } : new[] { argument, startIndex, length } ) { }
}
