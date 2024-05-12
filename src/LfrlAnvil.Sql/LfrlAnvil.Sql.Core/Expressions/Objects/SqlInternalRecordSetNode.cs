using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on another record set.
/// </summary>
public sealed class SqlInternalRecordSetNode : SqlRecordSetNode
{
    internal SqlInternalRecordSetNode(SqlRecordSetNode @base)
        : base( SqlNodeType.Unknown, alias: null, isOptional: @base.IsOptional )
    {
        Base = @base;
        Info = SqlRecordSetInfo.Create( "<internal>" );
    }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlRecordSetNode"/> instance.
    /// </summary>
    public SqlRecordSetNode Base { get; }

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        var knownFields = Base.GetKnownFields();
        if ( knownFields.Count == 0 )
            return knownFields;

        var i = 0;
        var result = new SqlDataFieldNode[knownFields.Count];
        foreach ( var field in knownFields )
            result[i++] = field.ReplaceRecordSet( this );

        return result;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return Base.GetUnsafeField( name ).ReplaceRecordSet( this );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetField(string name)
    {
        return Base.GetField( name ).ReplaceRecordSet( this );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlInternalRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional == optional ? this : new SqlInternalRecordSetNode( Base.MarkAsOptional( optional ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlInternalRecordSetNode AsSelf()
    {
        return this;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Aliased <see cref="SqlInternalRecordSetNode"/> instances are not supported.</exception>
    [Pure]
    public override SqlRecordSetNode As(string alias)
    {
        throw new NotSupportedException( ExceptionResources.InternalRecordSetsCannotBeAliased );
    }
}
