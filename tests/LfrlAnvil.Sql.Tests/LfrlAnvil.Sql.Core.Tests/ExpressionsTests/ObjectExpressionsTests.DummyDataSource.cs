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
        public void AddTrait_ShouldCreateDummyDataSourceWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.DummyDataSource();
            var trait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( trait );
                text.Should()
                    .Be(
                        @"FROM <DUMMY>
AND WHERE
    (a > 10)" );
            }
        }

        [Fact]
        public void AddTrait_ShouldCreateDummyDataSourceWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.DummyDataSource().AddTrait( firstTrait );
            var secondTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "b > 15" ), isConjunction: false );
            var result = sut.AddTrait( secondTrait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( firstTrait, secondTrait );
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
