using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.NumericsTests.IntegerPartitionTests;

[TestClass( typeof( IntegerPartitionTestsData ) )]
public class IntegerPartitionTests : TestsBase
{
    [Theory]
    [MethodData( nameof( IntegerPartitionTestsData.GetFractionData ) )]
    public void GetEnumerator_ShouldReturnCorrectResult_WhenPartCountIsGreaterThanZero(ulong value, Fraction[] parts, ulong[] expected)
    {
        var sut = new IntegerPartition( value, parts );

        using ( new AssertionScope() )
        {
            var sum = sut.Aggregate( 0UL, (a, b) => a + b );
            sut.Should().BeSequentiallyEqualTo( expected );
            sum.Should().Be( sut.Sum );
        }
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_WhenPartCountEqualsZero()
    {
        var sut = new IntegerPartition( Fixture.Create<ulong>() );
        sut.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_ForDefault()
    {
        var sut = default( IntegerPartition );
        sut.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenAtLeastOneFractionIsLessThanZero()
    {
        var action = Lambda.Of( () => new IntegerPartition( Fixture.Create<ulong>(), Fraction.One, Fraction.One, -Fraction.Epsilon ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenCommonDenominatorIsTooLarge()
    {
        var action = Lambda.Of( () => new IntegerPartition( Fixture.Create<ulong>(), new Fraction( 1, 2 ), Fraction.Epsilon ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenOneOfTheFractionsAlignedToCommonDenominatorHasTooLargeNumerator()
    {
        var action = Lambda.Of( () => new IntegerPartition( Fixture.Create<ulong>(), new Fraction( 1, 2 ), Fraction.MaxValue ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenCommonNumeratorSumIsTooLarge()
    {
        var action = Lambda.Of(
            () => new IntegerPartition( Fixture.Create<ulong>(), Fraction.MaxValue, Fraction.MaxValue, new Fraction( 2 ) ) );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenFinalSumComponentIsTooLarge()
    {
        var action = Lambda.Of( () => new IntegerPartition( ulong.MaxValue, new Fraction( 3, 2 ) ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new IntegerPartition( 1234, new Fraction( 1, 6 ), new Fraction( 1, 3 ), new Fraction( 1, 2 ), new Fraction( 1 ) );
        var result = sut.ToString();
        result.Should().Be( "Partition 1234 into 4 fraction part(s) with 2468 sum" );
    }
}
