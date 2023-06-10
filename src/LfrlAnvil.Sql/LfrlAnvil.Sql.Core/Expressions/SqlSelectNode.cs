using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlSelectNode : SqlNodeBase
{
    protected SqlSelectNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    public abstract SqlExpressionType? Type { get; }

    public abstract void RegisterKnownFields(SqlQueryRecordSetNode.FieldInitializer initializer);
}
