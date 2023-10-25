using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlRecordSetNode : SqlNodeBase
{
    private string? _identifier;

    protected SqlRecordSetNode(SqlNodeType nodeType, string? alias, bool isOptional)
        : base( nodeType )
    {
        IsOptional = isOptional;
        Alias = alias;
        _identifier = alias;
    }

    public bool IsOptional { get; }
    public string? Alias { get; }

    [MemberNotNullWhen( true, nameof( Alias ) )]
    public bool IsAliased => Alias is not null;

    // TODO: replace with SqlRecordSetInfo, since this doesn't work correctly for temp tables
    public string Identifier => _identifier ??= SourceSchemaName.Length > 0 ? $"{SourceSchemaName}.{SourceName}" : SourceName;
    public abstract string SourceSchemaName { get; }
    public abstract string SourceName { get; }

    public SqlDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public abstract IReadOnlyCollection<SqlDataFieldNode> GetKnownFields();

    [Pure]
    public abstract SqlDataFieldNode GetUnsafeField(string name);

    [Pure]
    public abstract SqlDataFieldNode GetField(string name);

    [Pure]
    public abstract SqlRecordSetNode As(string alias);

    [Pure]
    public abstract SqlRecordSetNode AsSelf();

    [Pure]
    public abstract SqlRecordSetNode MarkAsOptional(bool optional = true);
}
