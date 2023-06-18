namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlOrdinalCommonTableExpressionNode : SqlCommonTableExpressionNode
{
    internal SqlOrdinalCommonTableExpressionNode(SqlQueryExpressionNode query, string name)
        : base( query, name, isRecursive: false ) { }
}
