using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single data field of a query record set.
/// </summary>
public sealed class SqlQueryDataFieldNode : SqlDataFieldNode
{
    internal SqlQueryDataFieldNode(SqlRecordSetNode recordSet, string name, SqlSelectNode selection, SqlExpressionNode? expression)
        : base( recordSet, SqlNodeType.QueryDataField )
    {
        Name = name;
        Selection = selection;
        Expression = expression;
    }

    /// <summary>
    /// Source selection.
    /// </summary>
    public SqlSelectNode Selection { get; }

    /// <summary>
    /// Expression associated with this data field.
    /// </summary>S
    public SqlExpressionNode? Expression { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlQueryDataFieldNode( recordSet, Name, Selection, Expression );
    }
}
