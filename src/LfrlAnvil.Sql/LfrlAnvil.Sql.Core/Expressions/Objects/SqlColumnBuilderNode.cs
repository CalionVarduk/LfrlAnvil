using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlColumnBuilderNode : SqlDataFieldNode
{
    private readonly bool _isOptional;

    internal SqlColumnBuilderNode(SqlRecordSetNode recordSet, ISqlColumnBuilder value, bool isOptional)
        : base( recordSet, SqlNodeType.ColumnBuilder )
    {
        Value = value;
        _isOptional = isOptional;
    }

    public ISqlColumnBuilder Value { get; }
    public TypeNullability Type => TypeNullability.Create( Value.TypeDefinition.RuntimeType, _isOptional || Value.IsNullable );
    public override string Name => Value.Name;
}
