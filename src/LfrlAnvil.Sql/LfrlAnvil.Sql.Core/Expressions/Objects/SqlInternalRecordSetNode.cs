using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlInternalRecordSetNode : SqlRecordSetNode
{
    internal SqlInternalRecordSetNode(SqlRecordSetNode @base)
        : base( SqlNodeType.Unknown, alias: null, isOptional: @base.IsOptional )
    {
        Base = @base;
        Info = SqlRecordSetInfo.Create( "<internal>" );
    }

    public override SqlRecordSetInfo Info { get; }
    public SqlRecordSetNode Base { get; }

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

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return Base.GetUnsafeField( name ).ReplaceRecordSet( this );
    }

    [Pure]
    public override SqlDataFieldNode GetField(string name)
    {
        return Base.GetField( name ).ReplaceRecordSet( this );
    }

    [Pure]
    public override SqlInternalRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional == optional ? this : new SqlInternalRecordSetNode( Base.MarkAsOptional( optional ) );
    }

    [Pure]
    public override SqlInternalRecordSetNode AsSelf()
    {
        return this;
    }

    [Pure]
    public override SqlRecordSetNode As(string alias)
    {
        throw new NotSupportedException( ExceptionResources.InternalRecordSetsCannotBeAliased );
    }
}
