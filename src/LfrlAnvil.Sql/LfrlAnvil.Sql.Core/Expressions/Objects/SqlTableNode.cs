using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlTableNode : SqlRecordSetNode
{
    private Dictionary<string, SqlColumnNode>? _columns;

    internal SqlTableNode(ISqlTable table, string? alias, bool isOptional)
        : base( SqlNodeType.Table, alias, isOptional )
    {
        Table = table;
        _columns = null;
    }

    public ISqlTable Table { get; }
    public override string SourceSchemaName => Table.Schema.Name;
    public override string SourceName => Table.Name;
    public new SqlColumnNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlColumnNode> GetKnownFields()
    {
        _columns ??= CreateColumnFields();
        return _columns.Values;
    }

    [Pure]
    public override SqlTableNode As(string alias)
    {
        return new SqlTableNode( Table, alias, IsOptional );
    }

    [Pure]
    public override SqlTableNode AsSelf()
    {
        return new SqlTableNode( Table, alias: null, IsOptional );
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlColumnNode GetField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns[name];
    }

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
        var result = new Dictionary<string, SqlColumnNode>( capacity: columns.Count, comparer: StringComparer.OrdinalIgnoreCase );

        foreach ( var column in columns )
            result.Add( column.Name, new SqlColumnNode( this, column, IsOptional ) );

        return result;
    }
}
