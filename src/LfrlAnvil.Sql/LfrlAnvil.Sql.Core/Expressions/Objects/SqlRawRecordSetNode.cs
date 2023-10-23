using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public class SqlRawRecordSetNode : SqlRecordSetNode
{
    protected internal SqlRawRecordSetNode(string name, string? alias, bool isOptional)
        : base( SqlNodeType.RawRecordSet, alias, isOptional )
    {
        SourceName = name;
    }

    public sealed override string SourceSchemaName => string.Empty;
    public sealed override string SourceName { get; }
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    [Pure]
    public override SqlRawRecordSetNode As(string alias)
    {
        return new SqlRawRecordSetNode( SourceName, alias, IsOptional );
    }

    [Pure]
    public override SqlRawRecordSetNode AsSelf()
    {
        return new SqlRawRecordSetNode( SourceName, alias: null, IsOptional );
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
            ? new SqlRawRecordSetNode( SourceName, alias: Alias, isOptional: optional )
            : this;
    }
}
