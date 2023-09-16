using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class SingleDataSource : TestsBase
    {
        [Fact]
        public void GetRecordSet_ShouldReturnFrom_WhenNameEqualsFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var result = sut.GetRecordSet( "foo" );

            result.Should().BeSameAs( from );
        }

        [Fact]
        public void GetRecordSet_ShouldThrowKeyNotFoundException_WhenNameDoesNotEqualFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var action = Lambda.Of( () => sut.GetRecordSet( "bar" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetRecordSet()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var result = sut["foo"]["bar"];

            result.Should().BeEquivalentTo( from["bar"] );
        }

        [Fact]
        public void AddTrait_ShouldCreateSingleDataSourceWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource();
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
AND WHERE a > 10" );
            }
        }

        [Fact]
        public void AddTrait_ShouldCreateSingleDataSourceWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource().AddTrait( firstTrait );
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
AND WHERE a > 10
OR WHERE b > 15" );
            }
        }
    }
}
