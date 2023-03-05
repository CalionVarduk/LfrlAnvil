using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.NumericsTests.IntegerPartitionTests;

[TestClass( typeof( IntegerPartitionTestsData ) )]
public class IntegerFixedPartitionTests : TestsBase
{
    [Theory]
    [MethodData( nameof( IntegerPartitionTestsData.GetFixedData ) )]
    public void GetEnumerator_ShouldReturnCorrectResult_WhenPartCountIsGreaterThanZero(ulong value, int partCount, ulong[] expected)
    {
        var sut = new IntegerFixedPartition( value, partCount );
        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_WhenPartCountEqualsZero()
    {
        var sut = new IntegerFixedPartition( Fixture.Create<ulong>(), partCount: 0 );
        sut.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_ForDefault()
    {
        var sut = default( IntegerFixedPartition );
        sut.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenPartCountIsLessThanZero()
    {
        var action = Lambda.Of( () => new IntegerFixedPartition( Fixture.Create<ulong>(), partCount: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new IntegerFixedPartition( 123, 7 );
        var result = sut.ToString();
        result.Should().Be( "123 into 7 fixed part(s)" );
    }
}
