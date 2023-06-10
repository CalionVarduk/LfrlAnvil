using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlDataSourceNode : SqlNodeBase
{
    protected SqlDataSourceNode()
        : base( SqlNodeType.DataSource ) { }

    public abstract SqlRecordSetNode From { get; }
    public abstract ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins { get; }
    public abstract IReadOnlyCollection<SqlRecordSetNode> RecordSets { get; }
    public SqlRecordSetNode this[string recordSetName] => GetRecordSet( recordSetName );

    [Pure]
    public abstract SqlRecordSetNode GetRecordSet(string name);
}
