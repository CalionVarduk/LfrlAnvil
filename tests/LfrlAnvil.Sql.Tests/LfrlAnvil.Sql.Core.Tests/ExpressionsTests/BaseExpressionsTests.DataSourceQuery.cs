using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
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

            Assertion.All(
                    selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                    sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                    sut.Traits.ToArray().TestSequence( query.Traits ),
                    sut.DataSource.TestRefEquals( dataSource ),
                    sut.Selection.ToArray().TestSequence( [ oldSelection, newSelection ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        SELECT
                          ([foo].[bar] : ?),
                          ([foo].[qux] : ?) AS [x]
                        """ ) )
                .Go();
        }

        [Fact]
        public void Select_ShouldReturnSelf_WhenSelectionIsEmpty()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var oldSelection = dataSource.From.GetField( "bar" ).AsSelf();
            var query = dataSource.Select( oldSelection );

            var sut = query.Select();

            sut.TestRefEquals( query ).Go();
        }

        [Fact]
        public void AddTrait_ShouldCreateDataSourceQueryWithTrait_WhenCalledForTheFirstTime()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() );
            var trait = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ trait ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        LIMIT ("10" : System.Int32)
                        SELECT
                          *
                        """ ) )
                .Go();
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

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ firstTrait, secondTrait ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        LIMIT ("10" : System.Int32)
                        OFFSET ("15" : System.Int32)
                        SELECT
                          *
                        """ ) )
                .Go();
        }

        [Fact]
        public void SetTraits_ShouldCreateCompoundQueryWithOverriddenTraits()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AddTrait( SqlNode.DistinctTrait() );

            var traits = Chain.Create<SqlTraitNode>( SqlNode.LimitTrait( SqlNode.Literal( 10 ) ) )
                .Extend( SqlNode.OffsetTrait( SqlNode.Literal( 15 ) ) );

            var result = sut.SetTraits( traits );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( traits ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        LIMIT ("10" : System.Int32)
                        OFFSET ("15" : System.Int32)
                        SELECT
                          *
                        """ ) )
                .Go();
        }
    }
}
