using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a single expression selection.
/// </summary>
public sealed class SqlSelectFieldNode : SqlSelectNode
{
    internal SqlSelectFieldNode(SqlExpressionNode expression, string? alias)
        : base( SqlNodeType.SelectField )
    {
        Expression = expression;
        Alias = alias;
    }

    /// <summary>
    /// Selected expression.
    /// </summary>
    public SqlExpressionNode Expression { get; }

    /// <summary>
    /// Optional alias of the selected <see cref="Expression"/>.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Name of this selected field.
    /// </summary>
    /// <remarks>
    /// Equal to <see cref="Alias"/> when it is not null,
    /// otherwise equal to <see cref="SqlDataFieldNode.Name"/> of the underlying <see cref="SqlDataFieldNode"/> <see cref="Expression"/>.
    /// </remarks>
    public string FieldName => Alias ?? ReinterpretCast.To<SqlDataFieldNode>( Expression ).Name;

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        visitor.Handle( FieldName, Expression );
    }
}
