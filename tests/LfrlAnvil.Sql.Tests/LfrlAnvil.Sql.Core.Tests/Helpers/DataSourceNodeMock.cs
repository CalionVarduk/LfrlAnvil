using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class DataSourceNodeMock : SqlDataSourceNode
{
    private readonly SqlRecordSetNode[] _from;

    public DataSourceNodeMock(Chain<SqlTraitNode>? traits = null)
        : base( traits ?? Chain<SqlTraitNode>.Empty )
    {
        _from = new SqlRecordSetNode[] { SqlNode.RawRecordSet( "x" ) };
    }

    public override SqlRecordSetNode From => _from[0];
    public override ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins => ReadOnlyMemory<SqlDataSourceJoinOnNode>.Empty;
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => _from;

    public override SqlRecordSetNode GetRecordSet(string name)
    {
        return string.Equals( From.Identifier, name, StringComparison.OrdinalIgnoreCase ) ? From : throw new Exception();
    }

    public override SqlDataSourceNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new DataSourceNodeMock( traits );
    }
}
