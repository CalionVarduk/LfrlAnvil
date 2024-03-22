using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlRawDataFieldNode : SqlDataFieldNode
{
    internal SqlRawDataFieldNode(SqlRecordSetNode recordSet, string name, TypeNullability? type)
        : base( recordSet, SqlNodeType.RawDataField )
    {
        Name = name;
        Type = type;
    }

    public override string Name { get; }
    public TypeNullability? Type { get; }

    [Pure]
    public override SqlRawDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlRawDataFieldNode( recordSet, Name, Type );
    }
}
