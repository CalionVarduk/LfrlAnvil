using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlSingleDataSourceNode<TRecordSetNode> : SqlDataSourceNode
    where TRecordSetNode : SqlRecordSetNode
{
    private readonly TRecordSetNode[] _from;

    internal SqlSingleDataSourceNode(TRecordSetNode from)
        : base( Chain<SqlDataSourceDecoratorNode>.Empty )
    {
        _from = new[] { from };
    }

    private SqlSingleDataSourceNode(SqlSingleDataSourceNode<TRecordSetNode> @base, Chain<SqlDataSourceDecoratorNode> decorators)
        : base( decorators )
    {
        _from = @base._from;
    }

    public override TRecordSetNode From => _from[0];
    public override ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins => ReadOnlyMemory<SqlDataSourceJoinOnNode>.Empty;
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => _from;
    public new TRecordSetNode this[string name] => GetRecordSet( name );

    [Pure]
    public override TRecordSetNode GetRecordSet(string name)
    {
        return name.Equals( From.Name, StringComparison.OrdinalIgnoreCase )
            ? From
            : throw new KeyNotFoundException( ExceptionResources.GivenRecordSetWasNotPresentInDataSource( name ) );
    }

    [Pure]
    public override SqlSingleDataSourceNode<TRecordSetNode> Decorate(SqlDataSourceDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlSingleDataSourceNode<TRecordSetNode>( this, decorators );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder.Append( "FROM" ).Append( ' ' ), From, indent );

        foreach ( var decorator in Decorators )
            AppendTo( builder.Indent( indent ), decorator, indent );
    }
}
