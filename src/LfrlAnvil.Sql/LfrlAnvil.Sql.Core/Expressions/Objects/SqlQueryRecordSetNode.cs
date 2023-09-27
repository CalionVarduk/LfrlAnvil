﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlQueryRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlQueryRecordSetNode(SqlQueryExpressionNode query, string alias, bool isOptional)
        : base( SqlNodeType.QueryRecordSet, isOptional )
    {
        Query = query;
        Name = alias;
        _fields = null;
    }

    public SqlQueryExpressionNode Query { get; }
    public override string Name { get; }
    public override bool IsAliased => true;
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= Query.ExtractKnownDataFields( this );
        return _fields.Values;
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= Query.ExtractKnownDataFields( this );
        return _fields.TryGetValue( name, out var field ) ? field : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= Query.ExtractKnownDataFields( this );
        return _fields[name];
    }

    [Pure]
    public override SqlQueryRecordSetNode As(string alias)
    {
        return new SqlQueryRecordSetNode( Query, alias, IsOptional );
    }

    [Pure]
    public override SqlQueryRecordSetNode AsSelf()
    {
        return this;
    }

    [Pure]
    public override SqlQueryRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlQueryRecordSetNode( Query, Name, isOptional: optional )
            : this;
    }
}
