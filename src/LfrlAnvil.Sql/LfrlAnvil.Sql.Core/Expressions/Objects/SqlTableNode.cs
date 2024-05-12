using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="ISqlTable"/> instance.
/// </summary>
public sealed class SqlTableNode : SqlRecordSetNode
{
    private Dictionary<string, SqlColumnNode>? _columns;

    internal SqlTableNode(ISqlTable table, string? alias, bool isOptional)
        : base( SqlNodeType.Table, alias, isOptional )
    {
        Table = table;
        _columns = null;
    }

    /// <summary>
    /// Underlying <see cref="ISqlTable"/> instance.
    /// </summary>
    public ISqlTable Table { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => Table.Info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlColumnNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlColumnNode> GetKnownFields()
    {
        _columns ??= CreateColumnFields();
        return _columns.Values;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlTableNode As(string alias)
    {
        return new SqlTableNode( Table, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlTableNode AsSelf()
    {
        return new SqlTableNode( Table, alias: null, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlColumnNode GetField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns[name];
    }

    /// <inheritdoc />
    [Pure]
    public override SqlTableNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlTableNode( Table, Alias, isOptional: optional )
            : this;
    }

    [Pure]
    private Dictionary<string, SqlColumnNode> CreateColumnFields()
    {
        var columns = Table.Columns;
        var result = new Dictionary<string, SqlColumnNode>( capacity: columns.Count, comparer: SqlHelpers.NameComparer );

        foreach ( var column in columns )
            result.Add( column.Name, new SqlColumnNode( this, column, IsOptional ) );

        return result;
    }
}
