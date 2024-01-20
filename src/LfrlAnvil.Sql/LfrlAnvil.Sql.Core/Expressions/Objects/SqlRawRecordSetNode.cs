using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public class SqlRawRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    protected internal SqlRawRecordSetNode(string name, string? alias, bool isOptional)
        : this( SqlRecordSetInfo.Create( name ), alias, isOptional, isInfoRaw: true ) { }

    protected internal SqlRawRecordSetNode(SqlRecordSetInfo info, string? alias, bool isOptional)
        : this( info, alias, isOptional, isInfoRaw: false ) { }

    private SqlRawRecordSetNode(SqlRecordSetInfo info, string? alias, bool isOptional, bool isInfoRaw)
        : base( SqlNodeType.RawRecordSet, alias, isOptional )
    {
        _info = info;
        IsInfoRaw = isInfoRaw;
    }

    public sealed override SqlRecordSetInfo Info => _info;
    public bool IsInfoRaw { get; }
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    [Pure]
    public override SqlRawRecordSetNode As(string alias)
    {
        return new SqlRawRecordSetNode( Info, alias, IsOptional, IsInfoRaw );
    }

    [Pure]
    public override SqlRawRecordSetNode AsSelf()
    {
        return new SqlRawRecordSetNode( Info, alias: null, IsOptional, IsInfoRaw );
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
            ? new SqlRawRecordSetNode( Info, alias: Alias, isOptional: optional, IsInfoRaw )
            : this;
    }
}
