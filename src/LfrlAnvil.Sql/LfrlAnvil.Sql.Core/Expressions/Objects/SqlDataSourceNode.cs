using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlDataSourceNode : SqlNodeBase
{
    protected SqlDataSourceNode(Chain<SqlTraitNode> traits)
        : base( SqlNodeType.DataSource )
    {
        Traits = traits;
    }

    public Chain<SqlTraitNode> Traits { get; }
    public abstract SqlRecordSetNode From { get; }
    public abstract ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins { get; }
    public abstract IReadOnlyCollection<SqlRecordSetNode> RecordSets { get; }
    public SqlRecordSetNode this[string recordSetName] => GetRecordSet( recordSetName );

    [Pure]
    public abstract SqlRecordSetNode GetRecordSet(string name);

    [Pure]
    public abstract SqlDataSourceNode AddTrait(SqlTraitNode trait);
}
