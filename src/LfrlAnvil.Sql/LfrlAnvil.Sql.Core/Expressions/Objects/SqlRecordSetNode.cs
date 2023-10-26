using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlRecordSetNode : SqlNodeBase
{
    protected SqlRecordSetNode(SqlNodeType nodeType, string? alias, bool isOptional)
        : base( nodeType )
    {
        IsOptional = isOptional;
        Alias = alias;
    }

    public bool IsOptional { get; }
    public string? Alias { get; }
    public abstract SqlRecordSetInfo Info { get; }

    [MemberNotNullWhen( true, nameof( Alias ) )]
    public bool IsAliased => Alias is not null;

    public string Identifier => Alias ?? Info.Identifier;

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
