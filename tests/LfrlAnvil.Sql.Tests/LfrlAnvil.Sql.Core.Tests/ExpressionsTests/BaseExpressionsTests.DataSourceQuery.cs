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
                sut.Traits.ToArray().Should().BeSequentiallyEqualTo( query.Traits );
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
        public void AddTrait_ShouldCreateDataSourceQueryWithTrait_WhenCalledForTheFirstTime()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() );
            var trait = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( trait );
                text.Should()
                    .Be(
                        @"FROM [foo]
LIMIT (""10"" : System.Int32)
SELECT
  *" );
            }
        }

        [Fact]
        public void AddTrait_ShouldCreateCompoundQueryWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AddTrait( firstTrait );

            var secondTrait = SqlNode.OffsetTrait( SqlNode.Literal( 15 ) );
            var result = sut.AddTrait( secondTrait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( firstTrait, secondTrait );
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
