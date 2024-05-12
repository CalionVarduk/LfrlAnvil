using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a query selection of all <see cref="DataSource"/> fields.
/// </summary>
public sealed class SqlSelectAllNode : SqlSelectNode
{
    internal SqlSelectAllNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.SelectAll )
    {
        DataSource = dataSource;
    }

    /// <summary>
    /// Data source to select all data fields from.
    /// </summary>
    public SqlDataSourceNode DataSource { get; }

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        foreach ( var recordSet in DataSource.RecordSets )
        {
            foreach ( var field in recordSet.GetKnownFields() )
                visitor.Handle( field.Name, field );
        }
    }
}
