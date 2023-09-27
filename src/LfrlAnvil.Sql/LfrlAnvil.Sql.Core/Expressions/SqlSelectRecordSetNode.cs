using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectRecordSetNode : SqlSelectNode
{
    internal SqlSelectRecordSetNode(SqlRecordSetNode recordSet)
        : base( SqlNodeType.SelectRecordSet )
    {
        RecordSet = recordSet;
    }

    public SqlRecordSetNode RecordSet { get; }

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        foreach ( var field in RecordSet.GetKnownFields() )
            visitor.Handle( field.Name, field );
    }
}
