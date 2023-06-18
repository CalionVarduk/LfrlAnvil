using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlCommonTableExpressionNode : SqlNodeBase
{
    internal SqlCommonTableExpressionNode(SqlQueryExpressionNode query, string name, bool isRecursive)
        : base( SqlNodeType.CommonTableExpression )
    {
        Query = query;
        Name = name;
        IsRecursive = isRecursive;
        RecordSet = new SqlCommonTableExpressionRecordSetNode( this, alias: null, isOptional: false );
    }

    public SqlQueryExpressionNode Query { get; }
    public string Name { get; }
    public bool IsRecursive { get; }
    public SqlCommonTableExpressionRecordSetNode RecordSet { get; }

    protected sealed override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;

        builder
            .Append( IsRecursive ? "RECURSIVE" : "ORDINAL" )
            .Append( ' ' )
            .Append( '[' )
            .Append( Name )
            .Append( ']' )
            .Append( ' ' )
            .Append( '(' )
            .Indent( queryIndent );

        AppendTo( builder, Query, queryIndent );
        builder.Indent( indent ).Append( ')' );
    }
}
