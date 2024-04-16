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

        using ( new AssertionScope() )
        {
            sut.StartPoint.Should().Be( startPoint );
            sut.CurrentPoint.Should().Be( startPoint );
            sut.Count.Should().Be( 0 );
        }
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

        using ( new AssertionScope() )
        {
            sut.StartPoint.Should().Be( startPoint );
            sut.CurrentPoint.Should().Be( startPoint );
            sut.Count.Should().Be( 0 );
            sut.EventComparer.Should().BeSameAs( eventComparer );
        }
    }

    [Fact]
    public void AddDelta_ShouldReturnCorrectTimestamp()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.Move( Duration.FromTicks( 456 ) );
        sut.CurrentPoint.Should().Be( new Timestamp( 579 ) );
    }

    [Fact]
    public void SubtractDelta_ShouldReturnCorrectTimestamp()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.Enqueue( 0, Duration.FromTicks( 456 ) );

        var @event = sut.AdvanceDequeuePoint( 0, Duration.FromTicks( 321 ) );

        (@event?.DequeuePoint).Should().Be( new Timestamp( 258 ) );
    }

    [Fact]
    public void Add_ShouldReturnCorrectDuration()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.EnqueueInfinite( 0, Duration.FromTicks( 456 ) );

        var @event = sut.IncreaseDelta( 0, Duration.FromTicks( 321 ) );

        (@event?.Delta).Should().Be( Duration.FromTicks( 777 ) );
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectDuration()
    {
        var sut = new ReorderableEventQueue<int>( new Timestamp( 123 ) );
        sut.EnqueueInfinite( 0, Duration.FromTicks( 456 ) );

        var @event = sut.DecreaseDelta( 0, Duration.FromTicks( 321 ) );

        (@event?.Delta).Should().Be( Duration.FromTicks( 135 ) );
    }
}
