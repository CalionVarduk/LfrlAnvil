namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlSelectNode : SqlNodeBase
{
    internal SqlSelectNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    public abstract SqlExpressionType? Type { get; }

    internal abstract void Convert(ISqlSelectNodeConverter converter);
}
