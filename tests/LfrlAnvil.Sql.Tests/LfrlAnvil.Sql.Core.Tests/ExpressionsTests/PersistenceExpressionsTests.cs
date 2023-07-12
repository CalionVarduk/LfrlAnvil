using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class PersistenceExpressionsTests : TestsBase
{
    [Fact]
    public void DeleteFrom_ShouldCreateDeleteFromNode()
    {
        var set1 = SqlNode.RawRecordSet( "foo" );
        var set2 = SqlNode.RawRecordSet( "bar" );
        var dataSource = set1.Join( set2.InnerOn( set1["a"] == set2["b"] ) ).AndWhere( set2["c"] > SqlNode.Literal( 5 ) );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, SqlRecordSetNode>>();
        selector.WithAnyArgs( _ => set1 );
        var sut = dataSource.ToDeleteFrom( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DeleteFrom );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set1 );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (([foo].[a] : ?) = ([bar].[b] : ?))
AND WHERE
    (([bar].[c] : ?) > (""5"" : System.Int32))
DELETE [foo]" );
        }
    }

    [Fact]
    public void DeleteFrom_ShouldCreateDeleteFromNode_FromSingleDataSource()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var sut = dataSource.ToDeleteFrom();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DeleteFrom );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (([foo].[a] : ?) > (""5"" : System.Int32))
DELETE [foo]" );
        }
    }
}
