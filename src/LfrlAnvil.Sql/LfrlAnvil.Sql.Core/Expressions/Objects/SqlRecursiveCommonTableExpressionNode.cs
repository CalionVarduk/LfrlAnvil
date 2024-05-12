namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single recursive common table expression (CTE).
/// </summary>
public sealed class SqlRecursiveCommonTableExpressionNode : SqlCommonTableExpressionNode
{
    internal SqlRecursiveCommonTableExpressionNode(SqlCompoundQueryExpressionNode query, string name)
        : base( query, name, isRecursive: true ) { }

    /// <inheritdoc cref="SqlCommonTableExpressionNode.Query" />
    public new SqlCompoundQueryExpressionNode Query => ReinterpretCast.To<SqlCompoundQueryExpressionNode>( base.Query );
}
