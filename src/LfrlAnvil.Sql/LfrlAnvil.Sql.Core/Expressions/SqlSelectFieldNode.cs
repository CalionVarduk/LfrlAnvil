using System.Text;
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

    internal override void Convert(ISqlSelectNodeConverter converter)
    {
        converter.Add( FieldName, Expression );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder, Expression, indent );
        if ( Alias is not null )
            builder.Append( ' ' ).Append( "AS" ).Append( ' ' ).Append( '[' ).Append( Alias ).Append( ']' );
    }
}
