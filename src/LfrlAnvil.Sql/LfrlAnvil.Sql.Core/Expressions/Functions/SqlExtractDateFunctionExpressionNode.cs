namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlExtractDateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractDateFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ExtractDate, new[] { argument } ) { }
}
