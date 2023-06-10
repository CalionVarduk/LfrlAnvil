using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlRawQueryRecordSetNode : SqlRecordSetNode
{
    internal SqlRawQueryRecordSetNode(SqlRawQueryExpressionNode query, string alias, bool isOptional)
        : base( isOptional )
    {
        Query = query;
        Name = alias;
    }

    public SqlRawQueryExpressionNode Query { get; }
    public override string Name { get; }
    public override bool IsAliased => true;
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    [Pure]
    public override SqlRawDataFieldNode GetUnsafeField(string name)
    {
        return new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlRawDataFieldNode GetField(string name)
    {
        return new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlRawQueryRecordSetNode As(string alias)
    {
        return new SqlRawQueryRecordSetNode( Query, alias, IsOptional );
    }

    [Pure]
    public override SqlRawQueryRecordSetNode AsSelf()
    {
        return this;
    }

    [Pure]
    public override SqlRawQueryRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlRawQueryRecordSetNode( Query, Name, isOptional: optional )
            : this;
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;
        AppendTo( builder.Append( '(' ).Indent( queryIndent ), Query, queryIndent );
        builder.Indent( indent ).Append( ')' ).Append( ' ' ).Append( "AS" ).Append( ' ' ).Append( '[' ).Append( Name ).Append( ']' );
    }
}
