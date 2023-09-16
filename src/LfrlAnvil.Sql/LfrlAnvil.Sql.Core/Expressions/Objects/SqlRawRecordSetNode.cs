using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public class SqlRawRecordSetNode : SqlRecordSetNode
{
    protected internal SqlRawRecordSetNode(string name, string? alias, bool isOptional)
        : base( SqlNodeType.RawRecordSet, isOptional )
    {
        BaseName = name;
        Name = alias ?? name;
        IsAliased = alias is not null;
    }

    public string BaseName { get; }
    public sealed override string Name { get; }
    public sealed override bool IsAliased { get; }
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    [Pure]
    public override SqlRawRecordSetNode As(string alias)
    {
        return new SqlRawRecordSetNode( BaseName, alias, IsOptional );
    }

    [Pure]
    public override SqlRawRecordSetNode AsSelf()
    {
        return new SqlRawRecordSetNode( BaseName, alias: null, IsOptional );
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
    public override SqlRawRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlRawRecordSetNode( BaseName, alias: IsAliased ? Name : null, isOptional: optional )
            : this;
    }
}
