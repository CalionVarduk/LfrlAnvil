using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlRawDataFieldNode : SqlDataFieldNode
{
    internal SqlRawDataFieldNode(SqlRecordSetNode recordSet, string name, SqlExpressionType? type)
        : base( recordSet, SqlNodeType.RawDataField )
    {
        Name = name;
        Type = type;
    }

    public override string Name { get; }
    public SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        base.ToString( builder, indent );
        AppendTypeTo( builder, Type );
    }
}
