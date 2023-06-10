using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlRecordSetNode : SqlNodeBase
{
    protected SqlRecordSetNode(bool isOptional)
        : base( SqlNodeType.RecordSet )
    {
        IsOptional = isOptional;
    }

    public bool IsOptional { get; }
    public abstract string Name { get; }
    public abstract bool IsAliased { get; }
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
