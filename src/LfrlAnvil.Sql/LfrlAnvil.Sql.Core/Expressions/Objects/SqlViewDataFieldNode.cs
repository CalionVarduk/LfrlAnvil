using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlViewDataFieldNode : SqlDataFieldNode
{
    internal SqlViewDataFieldNode(SqlRecordSetNode recordSet, ISqlViewDataField value)
        : base( recordSet, SqlNodeType.ViewDataField )
    {
        Value = value;
    }

    public ISqlViewDataField Value { get; }
    public override string Name => Value.Name;
}
