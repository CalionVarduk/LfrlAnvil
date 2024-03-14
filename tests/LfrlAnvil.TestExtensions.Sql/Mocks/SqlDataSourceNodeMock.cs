using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDataSourceNodeMock : SqlDataSourceNode
{
    private readonly SqlRecordSetNode[] _from;

    public SqlDataSourceNodeMock(Chain<SqlTraitNode>? traits = null)
        : base( traits ?? Chain<SqlTraitNode>.Empty )
    {
        _from = new SqlRecordSetNode[] { SqlNode.RawRecordSet( "x" ) };
    }

    public override SqlRecordSetNode From => _from[0];
    public override ReadOnlyArray<SqlDataSourceJoinOnNode> Joins => ReadOnlyArray<SqlDataSourceJoinOnNode>.Empty;
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => _from;

    public override SqlRecordSetNode GetRecordSet(string name)
    {
        return string.Equals( From.Identifier, name, StringComparison.OrdinalIgnoreCase ) ? From : throw new Exception();
    }

    public override SqlDataSourceNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlDataSourceNodeMock( traits );
    }
}
