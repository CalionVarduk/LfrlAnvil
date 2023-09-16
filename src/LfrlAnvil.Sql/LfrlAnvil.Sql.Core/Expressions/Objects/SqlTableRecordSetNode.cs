using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlTableRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlColumnNode>? _columns;

    internal SqlTableRecordSetNode(ISqlTable table, string? alias, bool isOptional)
        : base( SqlNodeType.TableRecordSet, isOptional )
    {
        Table = table;
        Name = alias ?? Table.FullName;
        IsAliased = alias is not null;
        _columns = null;
    }

    public ISqlTable Table { get; }
    public override string Name { get; }
    public override bool IsAliased { get; }
    public new SqlColumnNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlColumnNode> GetKnownFields()
    {
        _columns ??= CreateColumnFields();
        return _columns.Values;
    }

    [Pure]
    public override SqlTableRecordSetNode As(string alias)
    {
        return new SqlTableRecordSetNode( Table, alias, IsOptional );
    }

    [Pure]
    public override SqlTableRecordSetNode AsSelf()
    {
        return new SqlTableRecordSetNode( Table, alias: null, IsOptional );
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
    public override SqlTableRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlTableRecordSetNode( Table, alias: IsAliased ? Name : null, isOptional: optional )
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
