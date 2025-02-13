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

        Assertion.All(
                sut.StartPoint.TestEquals( startPoint ),
                sut.CurrentPoint.TestEquals( startPoint ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void AddDelta_ShouldReturnCorrectTimestamp()
    {
        var sut = new EventQueue<int>( new Timestamp( 123 ) );
        sut.Move( Duration.FromTicks( 456 ) );
        sut.CurrentPoint.TestEquals( new Timestamp( 579 ) ).Go();
    }
}
