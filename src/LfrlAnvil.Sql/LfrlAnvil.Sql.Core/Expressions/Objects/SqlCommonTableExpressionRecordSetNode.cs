using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="SqlCommonTableExpressionNode"/> instance.
/// </summary>
public sealed class SqlCommonTableExpressionRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlCommonTableExpressionRecordSetNode(SqlCommonTableExpressionNode commonTableExpression, string? alias, bool isOptional)
        : base( SqlNodeType.CommonTableExpressionRecordSet, alias, isOptional )
    {
        _info = SqlRecordSetInfo.Create( commonTableExpression.Name );
        CommonTableExpression = commonTableExpression;
        _fields = null;
    }

    /// <summary>
    /// Underlying <see cref="SqlCommonTableExpressionNode"/> instance.
    /// </summary>
    public SqlCommonTableExpressionNode CommonTableExpression { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => _info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= CommonTableExpression.Query.ExtractKnownDataFields( this );
        return _fields.Values;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CommonTableExpression.Query.ExtractKnownDataFields( this );
        return _fields.TryGetValue( name, out var field ) ? field : new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= CommonTableExpression.Query.ExtractKnownDataFields( this );
        return _fields[name];
    }

    /// <inheritdoc />
    [Pure]
    public override SqlCommonTableExpressionRecordSetNode As(string alias)
    {
        return new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlCommonTableExpressionRecordSetNode AsSelf()
    {
        return IsAliased ? new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, alias: null, IsOptional ) : this;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlCommonTableExpressionRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, Alias, optional )
            : this;
    }
}
