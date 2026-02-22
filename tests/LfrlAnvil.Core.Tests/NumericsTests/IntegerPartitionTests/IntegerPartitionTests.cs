using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.NumericsTests.IntegerPartitionTests;

[TestClass( typeof( IntegerPartitionTestsData ) )]
public class IntegerPartitionTests : TestsBase
{
    [Theory]
    [MethodData( nameof( IntegerPartitionTestsData.GetFractionData ) )]
    public void GetEnumerator_ShouldReturnCorrectResult_WhenPartCountIsGreaterThanZero(ulong value, Fraction[] parts, ulong[] expected)
    {
        var sut = new IntegerPartition( value, parts );
        var sum = sut.Aggregate( 0UL, (a, b) => a + b );
        Assertion.All(
                sut.TestSequence( expected ),
                sum.TestEquals( sut.Sum ) )
            .Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_WhenPartCountEqualsZero()
    {
        var sut = new IntegerPartition( Fixture.Create<ulong>() );
        sut.TestEmpty().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_ForDefault()
    {
        var sut = default( IntegerPartition );
        sut.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenAtLeastOneFractionIsLessThanZero()
    {
        var action = Lambda.Of( () => new IntegerPartition( Fixture.Create<ulong>(), Fraction.One, Fraction.One, -Fraction.Epsilon ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenCommonDenominatorIsTooLarge()
    {
        var action = Lambda.Of( () => new IntegerPartition( Fixture.Create<ulong>(), new Fraction( 1, 2 ), Fraction.Epsilon ) );
        action.Test( exc => exc.TestType().Exact<OverflowException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenOneOfTheFractionsAlignedToCommonDenominatorHasTooLargeNumerator()
    {
        var action = Lambda.Of( () => new IntegerPartition( Fixture.Create<ulong>(), new Fraction( 1, 2 ), Fraction.MaxValue ) );
        action.Test( exc => exc.TestType().Exact<OverflowException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenCommonNumeratorSumIsTooLarge()
    {
        var action = Lambda.Of( () => new IntegerPartition(
            Fixture.Create<ulong>(),
            Fraction.MaxValue,
            Fraction.MaxValue,
            new Fraction( 2 ) ) );

        action.Test( exc => exc.TestType().Exact<OverflowException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowOverflowException_WhenFinalSumComponentIsTooLarge()
    {
        var action = Lambda.Of( () => new IntegerPartition( ulong.MaxValue, new Fraction( 3, 2 ) ) );
        action.Test( exc => exc.TestType().Exact<OverflowException>() ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new IntegerPartition( 1234, new Fraction( 1, 6 ), new Fraction( 1, 3 ), new Fraction( 1, 2 ), new Fraction( 1 ) );
        var result = sut.ToString();
        result.TestEquals( "Partition 1234 into 4 fraction part(s) with 2468 sum" ).Go();
    }
}
