using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.EnsureTests;

public class EnsureOfInt32Tests : GenericEnsureOfStructTypeTests<int>
{
    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( -1, 10 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 10, 10 )]
    [InlineData( 11, 10 )]
    [InlineData( 100, 20 )]
    public void IsInIndexRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsNotCorrectIndex(int param, int count)
    {
        var action = Lambda.Of( () => Ensure.IsInIndexRange( param, count ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 0, 10 )]
    [InlineData( 9, 10 )]
    [InlineData( 19, 20 )]
    public void IsInIndexRange_ShouldNotThrow_WhenParamIsCorrectIndex(int param, int count)
    {
        var action = Lambda.Of( () => Ensure.IsInIndexRange( param, count ) );
        action.Should().NotThrow();
    }
}
