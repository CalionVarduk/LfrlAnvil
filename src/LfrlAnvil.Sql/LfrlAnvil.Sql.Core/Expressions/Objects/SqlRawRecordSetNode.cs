using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public class SqlRawRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    protected internal SqlRawRecordSetNode(string name, string? alias, bool isOptional)
        : base( SqlNodeType.RawRecordSet, alias, isOptional )
    {
        _info = SqlRecordSetInfo.Create( name );
    }

    public sealed override SqlRecordSetInfo Info => _info;
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    [Pure]
    public override SqlRawRecordSetNode As(string alias)
    {
        return new SqlRawRecordSetNode( Info.Name.Object, alias, IsOptional );
    }

    [Pure]
    public override SqlRawRecordSetNode AsSelf()
    {
        return new SqlRawRecordSetNode( Info.Name.Object, alias: null, IsOptional );
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
            ? new SqlRawRecordSetNode( Info.Name.Object, alias: Alias, isOptional: optional )
            : this;
    }
}
