using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlColumnNode : SqlDataFieldNode
{
    internal SqlColumnNode(SqlRecordSetNode recordSet, ISqlColumn value, bool isOptional)
        : base( recordSet, SqlNodeType.Column )
    {
        Value = value;
        Type = SqlExpressionType.Create( Value.TypeDefinition.RuntimeType, isOptional || Value.IsNullable );
    }

    public ISqlColumn Value { get; }
    public SqlExpressionType Type { get; }
    public override string Name => Value.Name;
}
