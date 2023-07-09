using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class BaseExpressionsTests
{
    public class DataSourceQuery : TestsBase
    {
        [Fact]
        public void Select_ShouldCreateDataSourceQueryExpressionNode()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var oldSelection = dataSource.From["bar"].AsSelf();
            var newSelection = dataSource.From["qux"].As( "x" );
            var query = dataSource.Select( oldSelection );
            var selector = Substitute.For<Func<SqlDataSourceNode, IEnumerable<SqlSelectNode>>>();
            selector.WithAnyArgs( _ => new[] { newSelection } );
            var sut = query.Select( selector );
            var text = sut.ToString();

            using ( new AssertionScope() )
            {
                selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
                sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
                sut.Decorators.ToArray().Should().BeSequentiallyEqualTo( query.Decorators );
                sut.DataSource.Should().BeSameAs( dataSource );
                sut.Selection.ToArray().Should().BeSequentiallyEqualTo( oldSelection, newSelection );
                text.Should()
                    .Be(
                        @"FROM [foo]
SELECT
    ([foo].[bar] : ?),
    ([foo].[qux] : ?) AS [x]" );
            }
        }

        [Fact]
        public void Select_ShouldReturnSelf_WhenSelectionIsEmpty()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var oldSelection = dataSource.From.GetField( "bar" ).AsSelf();
            var query = dataSource.Select( oldSelection );

            var sut = query.Select();

            sut.Should().BeSameAs( query );
        }

        [Fact]
        public void Decorate_ShouldCreateDecoratedDataSourceQuery_WhenCalledForTheFirstTime()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() );
            var decorator = SqlNode.LimitDecorator( SqlNode.Literal( 10 ) );
            var result = sut.Decorate( decorator );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Decorators.Should().BeSequentiallyEqualTo( decorator );
                text.Should()
                    .Be(
                        @"FROM [foo]
LIMIT (""10"" : System.Int32)
SELECT
    *" );
            }
        }

        [Fact]
        public void Decorate_ShouldCreateDecoratedCompoundQuery_WhenCalledForTheSecondTime()
        {
            var firstDecorator = SqlNode.LimitDecorator( SqlNode.Literal( 10 ) );
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).Decorate( firstDecorator );

            var secondDecorator = SqlNode.OffsetDecorator( SqlNode.Literal( 15 ) );
            var result = sut.Decorate( secondDecorator );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Decorators.Should().BeSequentiallyEqualTo( firstDecorator, secondDecorator );
                text.Should()
                    .Be(
                        @"FROM [foo]
LIMIT (""10"" : System.Int32)
OFFSET (""15"" : System.Int32)
SELECT
    *" );
            }
        }
    }
}
