using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class MultiDataSource : TestsBase
    {
        [Fact]
        public void Ctor_ShouldThrowArgumentException_WhenMultipleRecordSetsWithSameNameExist()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var action = Lambda.Of( () => from.Join( from.InnerOn( SqlNode.True() ) ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Ctor_ShouldInitializeComplexDataSourceWithDifferentJoinTypes_ByMarkingRecordSetsAsOptionalCorrectly()
        {
            var a = SqlNode.RawRecordSet( "a" );
            var b = SqlNode.RawRecordSet( "b" );
            var c = SqlNode.RawRecordSet( "c" );
            var d = SqlNode.RawRecordSet( "d" );
            var e = SqlNode.RawRecordSet( "e" );
            var f = SqlNode.RawRecordSet( "f" );
            var g = SqlNode.RawRecordSet( "g" );
            var h = SqlNode.RawRecordSet( "h" );
            var ongoingSets = new List<SqlRecordSetNode[]>();
            var ongoingInners = new List<SqlRecordSetNode>();

            var sut = a.Join(
                SqlJoinDefinition.Inner( b, p => AddOngoing( p, "a" ) ),
                SqlJoinDefinition.Left( c, p => AddOngoing( p, "a", "b" ) ),
                SqlJoinDefinition.Inner( d, p => AddOngoing( p, "a", "b", "c" ) ),
                SqlJoinDefinition.Right( e, p => AddOngoing( p, "a", "b", "c", "d" ) ),
                SqlJoinDefinition.Inner( f, p => AddOngoing( p, "a", "b", "c", "d", "e" ) ),
                SqlJoinDefinition.Full( g, p => AddOngoing( p, "a", "b", "c", "d", "e", "f" ) ),
                SqlJoinDefinition.Cross( h ) );

            using ( new AssertionScope() )
            {
                sut.From.Should().BeEquivalentTo( a.MarkAsOptional() );

                sut.Joins.ToArray()
                    .Select( j => j.JoinType )
                    .Should()
                    .BeSequentiallyEqualTo(
                        SqlJoinType.Inner,
                        SqlJoinType.Left,
                        SqlJoinType.Inner,
                        SqlJoinType.Right,
                        SqlJoinType.Inner,
                        SqlJoinType.Full,
                        SqlJoinType.Cross );

                sut.Joins.ToArray().Select( j => j.InnerRecordSet ).Should().BeSequentiallyEqualTo( b, c, d, e, f, g, h );

                sut.GetRecordSet( "a" ).Should().BeEquivalentTo( a.MarkAsOptional() );
                sut.GetRecordSet( "b" ).Should().BeEquivalentTo( b.MarkAsOptional() );
                sut.GetRecordSet( "c" ).Should().BeEquivalentTo( c.MarkAsOptional() );
                sut.GetRecordSet( "d" ).Should().BeEquivalentTo( d.MarkAsOptional() );
                sut.GetRecordSet( "e" ).Should().BeEquivalentTo( e.MarkAsOptional() );
                sut.GetRecordSet( "f" ).Should().BeEquivalentTo( f.MarkAsOptional() );
                sut.GetRecordSet( "g" ).Should().BeEquivalentTo( g.MarkAsOptional() );
                sut.GetRecordSet( "h" ).Should().BeSameAs( h );

                ongoingInners.Should().BeSequentiallyEqualTo( b, c, d, e, f, g );

                ongoingSets.Should().HaveCount( 6 );
                ongoingSets.ElementAtOrDefault( 0 ).Should().BeEquivalentTo( a );
                ongoingSets.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( a, b );
                ongoingSets.ElementAtOrDefault( 2 ).Should().BeEquivalentTo( a, b, c.MarkAsOptional() );
                ongoingSets.ElementAtOrDefault( 3 ).Should().BeEquivalentTo( a, b, c.MarkAsOptional(), d );
                ongoingSets.ElementAtOrDefault( 4 )
                    .Should()
                    .BeEquivalentTo( a.MarkAsOptional(), b.MarkAsOptional(), c.MarkAsOptional(), d.MarkAsOptional(), e );

                ongoingSets.ElementAtOrDefault( 5 )
                    .Should()
                    .BeEquivalentTo( a.MarkAsOptional(), b.MarkAsOptional(), c.MarkAsOptional(), d.MarkAsOptional(), e, f );
            }

            SqlConditionNode AddOngoing(SqlJoinDefinition.ExpressionParams @params, params string[] outerSetNames)
            {
                ongoingInners.Add( @params.Inner );
                ongoingSets.Add( outerSetNames.Select( @params.GetOuter ).ToArray() );
                return SqlNode.True();
            }
        }

        [Fact]
        public void GetRecordSet_ShouldReturnFrom_WhenNameEqualsFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var result = sut.GetRecordSet( "foo" );

            result.Should().BeSameAs( from );
        }

        [Fact]
        public void GetRecordSet_ShouldReturnJoinedRecordSet_WhenNameEqualsItsName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var result = sut.GetRecordSet( "bar" );

            result.Should().BeSameAs( inner );
        }

        [Fact]
        public void GetRecordSet_ShouldThrowKeyNotFoundException_WhenNameDoesNotExist()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var action = Lambda.Of( () => sut.GetRecordSet( "qux" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Theory]
        [InlineData( "foo" )]
        [InlineData( "bar" )]
        public void Indexer_ShouldBeEquivalentToGetRecordSet(string name)
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var result = sut[name];

            result.Should().BeSameAs( sut.GetRecordSet( name ) );
        }

        [Fact]
        public void AddTrait_ShouldCreateMultiDataSourceWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
            var trait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( trait );
                text.Should()
                    .Be(
                        @"FROM [foo]
INNER JOIN [bar] ON TRUE
AND WHERE a > 10" );
            }
        }

        [Fact]
        public void AddTrait_ShouldCreateMultiDataSourceWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.RawRecordSet( "foo" )
                .Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) )
                .AddTrait( firstTrait );

            var secondTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "b > 15" ), isConjunction: false );
            var result = sut.AddTrait( secondTrait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( firstTrait, secondTrait );
                text.Should()
                    .Be(
                        @"FROM [foo]
INNER JOIN [bar] ON TRUE
AND WHERE a > 10
OR WHERE b > 15" );
            }
        }

        [Fact]
        public void SetTraits_ShouldCreateMultiDataSourceWithOverriddenTraits()
        {
            var sut = SqlNode.RawRecordSet( "foo" )
                .Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) )
                .AddTrait( SqlNode.DistinctTrait() );

            var traits = Chain.Create<SqlTraitNode>( SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true ) )
                .Extend( SqlNode.FilterTrait( SqlNode.RawCondition( "b > 15" ), isConjunction: false ) );

            var result = sut.SetTraits( traits );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( traits );
                text.Should()
                    .Be(
                        @"FROM [foo]
INNER JOIN [bar] ON TRUE
AND WHERE a > 10
OR WHERE b > 15" );
            }
        }
    }
}
