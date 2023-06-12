using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCompoundQueryComponentNode : SqlNodeBase
{
    internal SqlCompoundQueryComponentNode(SqlQueryExpressionNode query, SqlCompoundQueryOperator @operator)
        : base( SqlNodeType.CompoundQueryComponent )
    {
        Query = query;
        Operator = @operator;
    }

    public SqlQueryExpressionNode Query { get; }
    public SqlCompoundQueryOperator Operator { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;
        builder.Append( Operator.ToString().ToUpperInvariant() ).Indent( indent ).Append( '(' ).Indent( queryIndent );
        AppendTo( builder, Query, queryIndent );
        builder.Indent( indent ).Append( ')' );
    }
}
