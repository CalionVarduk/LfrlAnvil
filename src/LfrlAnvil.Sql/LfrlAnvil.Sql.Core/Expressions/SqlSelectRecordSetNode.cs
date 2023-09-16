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

    internal override void Convert(ISqlSelectNodeConverter converter)
    {
        foreach ( var field in RecordSet.GetKnownFields() )
            converter.Add( field.Name, field );
    }
}
