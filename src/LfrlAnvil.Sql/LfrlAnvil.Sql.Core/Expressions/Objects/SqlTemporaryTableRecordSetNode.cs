using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlTemporaryTableRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlRawDataFieldNode>? _columns;

    internal SqlTemporaryTableRecordSetNode(SqlCreateTemporaryTableNode creationNode, string? alias, bool isOptional)
        : base( SqlNodeType.TemporaryTableRecordSet, isOptional )
    {
        CreationNode = creationNode;
        Name = alias ?? CreationNode.Name;
        IsAliased = alias is not null;
        _columns = null;
    }

    public SqlCreateTemporaryTableNode CreationNode { get; }
    public override string Name { get; }
    public override bool IsAliased { get; }
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlRawDataFieldNode> GetKnownFields()
    {
        _columns ??= CreateColumnFields();
        return _columns.Values;
    }

    [Pure]
    public override SqlTemporaryTableRecordSetNode As(string alias)
    {
        return new SqlTemporaryTableRecordSetNode( CreationNode, alias, IsOptional );
    }

    [Pure]
    public override SqlTemporaryTableRecordSetNode AsSelf()
    {
        return new SqlTemporaryTableRecordSetNode( CreationNode, alias: null, IsOptional );
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlRawDataFieldNode GetField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns[name];
    }

    [Pure]
    public override SqlTemporaryTableRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlTemporaryTableRecordSetNode( CreationNode, alias: IsAliased ? Name : null, isOptional: optional )
            : this;
    }

    [Pure]
    private Dictionary<string, SqlRawDataFieldNode> CreateColumnFields()
    {
        var columns = CreationNode.Columns.Span;
        var result = new Dictionary<string, SqlRawDataFieldNode>( capacity: columns.Length, comparer: StringComparer.OrdinalIgnoreCase );

        foreach ( var column in columns )
            result.Add( column.Name, new SqlRawDataFieldNode( this, column.Name, IsOptional ? column.Type.MakeNullable() : column.Type ) );

        return result;
    }
}
