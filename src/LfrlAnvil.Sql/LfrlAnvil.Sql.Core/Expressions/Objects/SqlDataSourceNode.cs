using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlDataSourceNode : SqlNodeBase
{
    protected SqlDataSourceNode(Chain<SqlDataSourceTraitNode> traits)
        : base( SqlNodeType.DataSource )
    {
        Traits = traits;
    }

    public Chain<SqlDataSourceTraitNode> Traits { get; }
    public abstract SqlRecordSetNode From { get; }
    public abstract ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins { get; }
    public abstract IReadOnlyCollection<SqlRecordSetNode> RecordSets { get; }
    public SqlRecordSetNode this[string recordSetName] => GetRecordSet( recordSetName );

    [Pure]
    public abstract SqlRecordSetNode GetRecordSet(string name);

    [Pure]
    public abstract SqlDataSourceNode AddTrait(SqlDataSourceTraitNode trait);
}
