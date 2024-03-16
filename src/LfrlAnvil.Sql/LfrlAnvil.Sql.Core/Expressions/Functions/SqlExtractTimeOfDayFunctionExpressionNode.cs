namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlExtractTimeOfDayFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractTimeOfDayFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ExtractTimeOfDay, new[] { argument } ) { }
}
