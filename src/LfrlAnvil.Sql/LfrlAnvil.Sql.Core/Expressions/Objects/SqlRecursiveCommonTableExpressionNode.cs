namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlRecursiveCommonTableExpressionNode : SqlCommonTableExpressionNode
{
    internal SqlRecursiveCommonTableExpressionNode(SqlCompoundQueryExpressionNode query, string name)
        : base( query, name, isRecursive: true ) { }

    public new SqlCompoundQueryExpressionNode Query => ReinterpretCast.To<SqlCompoundQueryExpressionNode>( base.Query );
}
