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
        public void Decorate_ShouldCreateDecoratedSingleDataSource_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var decorator = SqlNode.FilterDecorator( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var result = sut.Decorate( decorator );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Decorators.Should().BeSequentiallyEqualTo( decorator );
                text.Should()
                    .Be(
                        @"FROM [foo]
AND WHERE
    (a > 10)" );
            }
        }

        [Fact]
        public void Decorate_ShouldCreateDecoratedMultiDataSource_WhenCalledForTheSecondTime()
        {
            var firstDecorator = SqlNode.FilterDecorator( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource().Decorate( firstDecorator );
            var secondDecorator = SqlNode.FilterDecorator( SqlNode.RawCondition( "b > 15" ), isConjunction: false );
            var result = sut.Decorate( secondDecorator );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Decorators.Should().BeSequentiallyEqualTo( firstDecorator, secondDecorator );
                text.Should()
                    .Be(
                        @"FROM [foo]
AND WHERE
    (a > 10)
OR WHERE
    (b > 15)" );
            }
        }
    }
}
