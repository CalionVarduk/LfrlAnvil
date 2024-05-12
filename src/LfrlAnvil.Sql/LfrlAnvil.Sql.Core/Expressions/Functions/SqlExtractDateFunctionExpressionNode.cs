namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that extracts the date part
/// from its parameter.
/// </summary>
public sealed class SqlExtractDateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractDateFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ExtractDate, new[] { argument } ) { }
}
