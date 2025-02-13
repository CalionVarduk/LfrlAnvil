using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class ReorderableEventQueueTests : TestsBase
{
    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_ShouldCreateWithCorrectStartAndCurrentPoints(long startTicks)
    {
        var startPoint = new Timestamp( startTicks );
        var sut = new ReorderableEventQueue<int>( startPoint );

        Assertion.All(
                sut.StartPoint.TestEquals( startPoint ),
                sut.CurrentPoint.TestEquals( startPoint ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_WithExplicitComparer_ShouldCreateWithCorrectStartAndCurrentPoints(long startTicks)
    {
        var startPoint = new Timestamp( startTicks );
        var eventComparer = EqualityComparerFactory<int>.Create( (a, b) => a.GetHashCode() == b.GetHashCode() );
        var sut = new ReorderableEventQueue<int>( startPoint, eventComparer );

        Assertion.All(
                sut.StartPoint.TestEquals( startPoint ),
                sut.CurrentPoint.TestEquals( startPoint ),
                sut.Count.TestEquals( 0 ),
                sut.EventComparer.TestRefEquals( eventComparer ) )
            .Go();
    }

    [Fact]
    public void AddDelta_ShouldReturnCorrectTimestamp()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.Move( Duration.FromTicks( 456 ) );
        sut.CurrentPoint.TestEquals( new Timestamp( 579 ) ).Go();
    }

    [Fact]
    public void SubtractDelta_ShouldReturnCorrectTimestamp()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.Enqueue( 0, Duration.FromTicks( 456 ) );

        var @event = sut.AdvanceDequeuePoint( 0, Duration.FromTicks( 321 ) );

        (@event?.DequeuePoint).TestEquals( new Timestamp( 258 ) ).Go();
    }

    [Fact]
    public void Add_ShouldReturnCorrectDuration()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.EnqueueInfinite( 0, Duration.FromTicks( 456 ) );

        var @event = sut.IncreaseDelta( 0, Duration.FromTicks( 321 ) );

        (@event?.Delta).TestEquals( Duration.FromTicks( 777 ) ).Go();
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectDuration()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.EnqueueInfinite( 0, Duration.FromTicks( 456 ) );

        var @event = sut.DecreaseDelta( 0, Duration.FromTicks( 321 ) );

        (@event?.Delta).TestEquals( Duration.FromTicks( 135 ) ).Go();
    }
}
