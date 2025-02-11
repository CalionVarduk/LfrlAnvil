using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.NumericsTests.IntegerPartitionTests;

[TestClass( typeof( IntegerPartitionTestsData ) )]
public class IntegerFixedPartitionTests : TestsBase
{
    [Theory]
    [MethodData( nameof( IntegerPartitionTestsData.GetFixedData ) )]
    public void GetEnumerator_ShouldReturnCorrectResult_WhenPartCountIsGreaterThanZero(ulong value, int partCount, ulong[] expected)
    {
        var sut = new IntegerFixedPartition( value, partCount );
        var sum = sut.Aggregate( 0UL, (a, b) => a + b );
        Assertion.All(
                sut.TestSequence( expected ),
                sum.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_WhenPartCountEqualsZero()
    {
        var sut = new IntegerFixedPartition( Fixture.Create<ulong>(), partCount: 0 );
        sut.TestEmpty().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_ForDefault()
    {
        var sut = default( IntegerFixedPartition );
        sut.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenPartCountIsLessThanZero()
    {
        var action = Lambda.Of( () => new IntegerFixedPartition( Fixture.Create<ulong>(), partCount: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new IntegerFixedPartition( 123, 7 );
        var result = sut.ToString();
        result.TestEquals( "Partition 123 into 7 fixed part(s)" ).Go();
    }
}
