using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class EventQueueTests : TestsBase
{
    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_ShouldCreateWithCorrectStartAndCurrentPoints(long startTicks)
    {
        var startPoint = new Timestamp( startTicks );
        var sut = new EventQueue<int>( startPoint );

        using ( new AssertionScope() )
        {
            sut.StartPoint.Should().Be( startPoint );
            sut.CurrentPoint.Should().Be( startPoint );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void AddDelta_ShouldReturnCorrectTimestamp()
    {
        var sut = new EventQueue<int>( new Timestamp( 123 ) );
        sut.Move( Duration.FromTicks( 456 ) );
        sut.CurrentPoint.Should().Be( new Timestamp( 579 ) );
    }
}
