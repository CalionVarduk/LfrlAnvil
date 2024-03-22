using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlColumnNode : SqlDataFieldNode
{
    internal SqlColumnNode(SqlRecordSetNode recordSet, ISqlColumn value, bool isOptional)
        : base( recordSet, SqlNodeType.Column )
    {
        Value = value;
        Type = TypeNullability.Create( Value.TypeDefinition.RuntimeType, isOptional || Value.IsNullable );
    }

    public ISqlColumn Value { get; }
    public TypeNullability Type { get; }
    public override string Name => Value.Name;

    [Pure]
    public override SqlColumnNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlColumnNode( recordSet, Value, Type.IsNullable );
    }
}
