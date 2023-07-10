using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class DummyDataSource : TestsBase
    {
        [Fact]
        public void From_ShouldThrowInvalidOperationException()
        {
            var sut = SqlNode.DummyDataSource();
            var action = Lambda.Of( () => sut.From );
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void GetRecordSet_ShouldThrowInvalidOperationException()
        {
            var sut = SqlNode.DummyDataSource();
            var action = Lambda.Of( () => sut.GetRecordSet( "foo" ) );
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Decorate_ShouldCreateDecoratedDummyDataSource_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.DummyDataSource();
            var decorator = SqlNode.FilterDecorator( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var result = sut.Decorate( decorator );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Decorators.Should().BeSequentiallyEqualTo( decorator );
                text.Should()
                    .Be(
                        @"FROM <DUMMY>
AND WHERE
    (a > 10)" );
            }
        }

        [Fact]
        public void Decorate_ShouldCreateDecoratedDummyDataSource_WhenCalledForTheSecondTime()
        {
            var firstDecorator = SqlNode.FilterDecorator( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.DummyDataSource().Decorate( firstDecorator );
            var secondDecorator = SqlNode.FilterDecorator( SqlNode.RawCondition( "b > 15" ), isConjunction: false );
            var result = sut.Decorate( secondDecorator );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Decorators.Should().BeSequentiallyEqualTo( firstDecorator, secondDecorator );
                text.Should()
                    .Be(
                        @"FROM <DUMMY>
AND WHERE
    (a > 10)
OR WHERE
    (b > 15)" );
            }
        }
    }
}
