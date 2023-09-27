using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectFieldNode : SqlSelectNode
{
    internal SqlSelectFieldNode(SqlExpressionNode expression, string? alias)
        : base( SqlNodeType.SelectField )
    {
        Expression = expression;
        Alias = alias;
    }

    public SqlExpressionNode Expression { get; }
    public string? Alias { get; }
    public string FieldName => Alias ?? ReinterpretCast.To<SqlDataFieldNode>( Expression ).Name;

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        visitor.Handle( FieldName, Expression );
    }
}
