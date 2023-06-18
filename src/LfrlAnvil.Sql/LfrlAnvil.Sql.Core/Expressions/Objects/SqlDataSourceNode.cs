using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlDataSourceNode : SqlNodeBase
{
    protected SqlDataSourceNode(Chain<SqlDataSourceDecoratorNode> decorators)
        : base( SqlNodeType.DataSource )
    {
        Decorators = decorators;
    }

    public Chain<SqlDataSourceDecoratorNode> Decorators { get; }
    public abstract SqlRecordSetNode From { get; }
    public abstract ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins { get; }
    public abstract IReadOnlyCollection<SqlRecordSetNode> RecordSets { get; }
    public SqlRecordSetNode this[string recordSetName] => GetRecordSet( recordSetName );

    [Pure]
    public abstract SqlRecordSetNode GetRecordSet(string name);

    [Pure]
    public abstract SqlDataSourceNode Decorate(SqlDataSourceDecoratorNode decorator);
}
