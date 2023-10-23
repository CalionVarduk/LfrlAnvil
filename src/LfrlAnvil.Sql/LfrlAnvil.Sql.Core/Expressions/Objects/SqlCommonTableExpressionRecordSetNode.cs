﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlCommonTableExpressionRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlCommonTableExpressionRecordSetNode(SqlCommonTableExpressionNode commonTableExpression, string? alias, bool isOptional)
        : base( SqlNodeType.CommonTableExpressionRecordSet, alias, isOptional )
    {
        CommonTableExpression = commonTableExpression;
        _fields = null;
    }

    public SqlCommonTableExpressionNode CommonTableExpression { get; }
    public override string SourceSchemaName => string.Empty;
    public override string SourceName => CommonTableExpression.Name;
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= CommonTableExpression.Query.ExtractKnownDataFields( this );
        return _fields.Values;
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CommonTableExpression.Query.ExtractKnownDataFields( this );
        return _fields.TryGetValue( name, out var field ) ? field : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= CommonTableExpression.Query.ExtractKnownDataFields( this );
        return _fields[name];
    }

    [Pure]
    public override SqlCommonTableExpressionRecordSetNode As(string alias)
    {
        return new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, alias, IsOptional );
    }

    [Pure]
    public override SqlCommonTableExpressionRecordSetNode AsSelf()
    {
        return IsAliased ? new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, alias: null, IsOptional ) : this;
    }

    [Pure]
    public override SqlCommonTableExpressionRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, Alias, optional )
            : this;
    }
}
