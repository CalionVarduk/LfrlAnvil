using LfrlAnvil.Numerics;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.NumericsTests.IntegerPartitionTests;

[TestClass( typeof( IntegerPartitionTestsData ) )]
public class IntegerFixedPartitionTests : TestsBase
{
    [Theory]
    [MethodData( nameof( IntegerPartitionTestsData.GetFixedData ) )]
    public void GetEnumerator_ShouldReturnCorrectResult_WhenPartCountIsGreaterThanZero(ulong value, ulong partCount, ulong[] expected)
    {
        var result = new IntegerFixedPartition( value, partCount );
        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_WhenPartCountEqualsZero()
    {
        var result = new IntegerFixedPartition( Fixture.Create<ulong>(), partCount: 0 );
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyResult_ForDefault()
    {
        var result = default( IntegerFixedPartition );
        result.Should().BeEmpty();
    }
}
