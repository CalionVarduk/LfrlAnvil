namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a single query selection.
/// </summary>
public abstract class SqlSelectNode : SqlNodeBase
{
    internal SqlSelectNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    internal abstract void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor);
}
