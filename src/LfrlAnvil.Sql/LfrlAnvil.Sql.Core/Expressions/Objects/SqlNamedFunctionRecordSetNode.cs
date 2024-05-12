using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on a named table-valued function.
/// </summary>
public sealed class SqlNamedFunctionRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    internal SqlNamedFunctionRecordSetNode(SqlNamedFunctionExpressionNode function, string alias, bool isOptional)
        : base( SqlNodeType.NamedFunctionRecordSet, alias, isOptional )
    {
        _info = SqlRecordSetInfo.Create( alias );
        Function = function;
    }

    /// <summary>
    /// Underlying <see cref="SqlNamedFunctionExpressionNode"/> instance.
    /// </summary>
    public SqlNamedFunctionExpressionNode Function { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => _info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <summary>
    /// Alias of this record set.
    /// </summary>
    public new string Alias
    {
        get
        {
            Assume.IsNotNull( base.Alias );
            return base.Alias;
        }
    }

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedFunctionRecordSetNode As(string alias)
    {
        return new SqlNamedFunctionRecordSetNode( Function, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedFunctionRecordSetNode AsSelf()
    {
        return this;
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
    public override SqlNamedFunctionRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNamedFunctionRecordSetNode( Function, Alias, isOptional: optional )
            : this;
    }
}
