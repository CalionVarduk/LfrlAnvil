using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlNewTableNode : SqlRecordSetNode
{
    private Dictionary<string, SqlRawDataFieldNode>? _columns;

    internal SqlNewTableNode(SqlCreateTableNode creationNode, string? alias, bool isOptional)
        : base( SqlNodeType.NewTable, alias, isOptional )
    {
        CreationNode = creationNode;
        _columns = null;
    }

    public SqlCreateTableNode CreationNode { get; }
    public override SqlRecordSetInfo Info => CreationNode.Info;
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlRawDataFieldNode> GetKnownFields()
    {
        _columns ??= CreateColumnFields();
        return _columns.Values;
    }

    [Pure]
    public override SqlNewTableNode As(string alias)
    {
        return new SqlNewTableNode( CreationNode, alias, IsOptional );
    }

    [Pure]
    public override SqlNewTableNode AsSelf()
    {
        return new SqlNewTableNode( CreationNode, alias: null, IsOptional );
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
    public override SqlNewTableNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNewTableNode( CreationNode, alias: Alias, isOptional: optional )
            : this;
    }

    [Pure]
    private Dictionary<string, SqlRawDataFieldNode> CreateColumnFields()
    {
        var columns = CreationNode.Columns.Span;
        var result = new Dictionary<string, SqlRawDataFieldNode>( capacity: columns.Length, comparer: SqlHelpers.NameComparer );

        foreach ( var column in columns )
            result.Add( column.Name, new SqlRawDataFieldNode( this, column.Name, IsOptional ? column.Type.MakeNullable() : column.Type ) );

        return result;
    }
}
