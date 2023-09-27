using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectAllNode : SqlSelectNode
{
    internal SqlSelectAllNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.SelectAll )
    {
        DataSource = dataSource;
    }

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
