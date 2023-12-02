using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlDataFieldNode : SqlExpressionNode
{
    protected SqlDataFieldNode(SqlRecordSetNode recordSet, SqlNodeType nodeType)
        : base( nodeType )
    {
        RecordSet = recordSet;
    }

    public SqlRecordSetNode RecordSet { get; }
    public abstract string Name { get; }

    [Pure]
    public static implicit operator SqlSelectFieldNode(SqlDataFieldNode node)
    {
        return node.AsSelf();
    }
}
