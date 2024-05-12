using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set without a source.
/// </summary>
public class SqlRawRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    /// <summary>
    /// Creates a new <see cref="SqlRawRecordSetNode"/> instance marked as raw.
    /// </summary>
    /// <param name="name">Raw name of this record set.</param>
    /// <param name="alias">Optional alias of this record set.</param>
    /// <param name="isOptional">Specifies whether or not this record set is marked as optional.</param>
    protected internal SqlRawRecordSetNode(string name, string? alias, bool isOptional)
        : this( SqlRecordSetInfo.Create( name ), alias, isOptional, isInfoRaw: true ) { }

    /// <summary>
    /// Creates a new <see cref="SqlRawRecordSetNode"/> instance.
    /// </summary>
    /// <param name="info"><see cref="SqlRecordSetInfo"/> associated with this record set.</param>
    /// <param name="alias">Optional alias of this record set.</param>
    /// <param name="isOptional">Specifies whether or not this record set is marked as optional.</param>
    protected internal SqlRawRecordSetNode(SqlRecordSetInfo info, string? alias, bool isOptional)
        : this( info, alias, isOptional, isInfoRaw: false ) { }

    private SqlRawRecordSetNode(SqlRecordSetInfo info, string? alias, bool isOptional, bool isInfoRaw)
        : base( SqlNodeType.RawRecordSet, alias, isOptional )
    {
        _info = info;
        IsInfoRaw = isInfoRaw;
    }

    /// <inheritdoc />
    public sealed override SqlRecordSetInfo Info => _info;

    /// <summary>
    /// Specifies whether or not this record set has been created with a <see cref="String"/> name
    /// rather than <see cref="SqlRecordSetInfo"/>.
    /// </summary>
    public bool IsInfoRaw { get; }

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawRecordSetNode As(string alias)
    {
        return new SqlRawRecordSetNode( Info, alias, IsOptional, IsInfoRaw );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawRecordSetNode AsSelf()
    {
        return new SqlRawRecordSetNode( Info, alias: null, IsOptional, IsInfoRaw );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawDataFieldNode GetUnsafeField(string name)
    {
        return new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawDataFieldNode GetField(string name)
    {
        return new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlRawRecordSetNode( Info, alias: Alias, isOptional: optional, IsInfoRaw )
            : this;
    }
}
