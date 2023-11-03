using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlNamedFunctionRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    internal SqlNamedFunctionRecordSetNode(SqlNamedFunctionExpressionNode function, string alias, bool isOptional)
        : base( SqlNodeType.NamedFunctionRecordSet, alias, isOptional )
    {
        _info = SqlRecordSetInfo.Create( alias );
        Function = function;
    }

    public SqlNamedFunctionExpressionNode Function { get; }
    public override SqlRecordSetInfo Info => _info;
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    public new string Alias
    {
        get
        {
            Assume.IsNotNull( base.Alias );
            return base.Alias;
        }
    }

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    [Pure]
    public override SqlNamedFunctionRecordSetNode As(string alias)
    {
        return new SqlNamedFunctionRecordSetNode( Function, alias, IsOptional );
    }

    [Pure]
    public override SqlNamedFunctionRecordSetNode AsSelf()
    {
        return this;
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
    public override SqlNamedFunctionRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNamedFunctionRecordSetNode( Function, Alias, isOptional: optional )
            : this;
    }
}
