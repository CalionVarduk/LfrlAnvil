﻿using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlDataSourceJoinOnNode : SqlNodeBase
{
    internal SqlDataSourceJoinOnNode(
        SqlJoinType joinType,
        SqlRecordSetNode innerRecordSet,
        SqlConditionNode onExpression)
        : base( SqlNodeType.JoinOn )
    {
        JoinType = joinType;
        InnerRecordSet = innerRecordSet;
        OnExpression = onExpression;
    }

    public SqlJoinType JoinType { get; }
    public SqlRecordSetNode InnerRecordSet { get; }
    public SqlConditionNode OnExpression { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var onIndent = indent + DefaultIndent;

        builder.Append( JoinType.ToString().ToUpperInvariant() ).Append( ' ' ).Append( "JOIN" ).Append( ' ' );
        AppendTo( builder, InnerRecordSet, indent );

        if ( JoinType != SqlJoinType.Cross )
        {
            builder.Indent( onIndent ).Append( "ON" ).Append( ' ' );
            AppendChildTo( builder, OnExpression, onIndent );
        }
    }
}
