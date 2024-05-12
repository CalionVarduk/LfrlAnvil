using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a query selection of all <see cref="RecordSet"/> fields.
/// </summary>
public sealed class SqlSelectRecordSetNode : SqlSelectNode
{
    internal SqlSelectRecordSetNode(SqlRecordSetNode recordSet)
        : base( SqlNodeType.SelectRecordSet )
    {
        RecordSet = recordSet;
    }

    /// <summary>
    /// Single record set to select all data fields from.
    /// </summary>
    public SqlRecordSetNode RecordSet { get; }

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        foreach ( var field in RecordSet.GetKnownFields() )
            visitor.Handle( field.Name, field );
    }
}
