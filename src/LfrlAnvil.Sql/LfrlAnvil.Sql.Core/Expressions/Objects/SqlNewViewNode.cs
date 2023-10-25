using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlNewViewNode : SqlRecordSetNode
{
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlNewViewNode(SqlCreateViewNode creationNode, string? alias, bool isOptional)
        : base( SqlNodeType.NewView, alias, isOptional )
    {
        CreationNode = creationNode;
        _fields = null;
    }

    public SqlCreateViewNode CreationNode { get; }
    public override string SourceSchemaName => CreationNode.Info.Name.Schema;
    public override string SourceName => CreationNode.Info.Name.Object;

    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= CreationNode.Source.ExtractKnownDataFields( this );
        return _fields.Values;
    }

    [Pure]
    public override SqlNewViewNode As(string alias)
    {
        return new SqlNewViewNode( CreationNode, alias, IsOptional );
    }

    [Pure]
    public override SqlNewViewNode AsSelf()
    {
        return new SqlNewViewNode( CreationNode, alias: null, IsOptional );
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CreationNode.Source.ExtractKnownDataFields( this );
        return _fields.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= CreationNode.Source.ExtractKnownDataFields( this );
        return _fields[name];
    }

    [Pure]
    public override SqlNewViewNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNewViewNode( CreationNode, alias: Alias, isOptional: optional )
            : this;
    }
}
