using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.ToTaskTests;

public class ToTaskTests : TestsBase
{
    [Fact]
    public void ToTaskExtension_ShouldCreateNewTaskThatWaitsForActivation()
    {
        var source = new EventPublisher<int>();
        var task = source.ToTask( CancellationToken.None );

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeTrue();
            task.Status.Should().Be( TaskStatus.WaitingForActivation );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnCancelledTask_WhenCancellationTokenHasRequestedCancellation()
    {
        var source = new EventPublisher<int>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var task = source.ToTask( cancellationTokenSource.Token );

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            task.Status.Should().Be( TaskStatus.Canceled );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnTaskThatDoesNotFinish_WhenSourcePublishesAnEventButDoesNotDispose()
    {
        var source = new EventPublisher<int>();
        var task = source.ToTask( CancellationToken.None );

        source.Publish( Fixture.Create<int>() );

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeTrue();
            task.Status.Should().Be( TaskStatus.WaitingForActivation );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnTaskThatGetsCancelled_WhenSourcePublishesAnEventButCancellationHasBeenRequested()
    {
        var source = new EventPublisher<int>();
        var cancellationTokenSource = new CancellationTokenSource();

        var task = source.ToTask( cancellationTokenSource.Token );

        source.Publish( Fixture.Create<int>() );
        cancellationTokenSource.Cancel();

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            task.Status.Should().Be( TaskStatus.Canceled );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnTaskThatRunsToCompletionWithLastEvent_WhenSourcePublishesEventsAndDisposes()
    {
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var expectedResult = values[^1];
        var source = new EventPublisher<int>();
        var task = source.ToTask( CancellationToken.None );

        foreach ( var e in values )
            source.Publish( e );

        source.Dispose();

        using ( new AssertionScope() )
        {
            task.Status.Should().Be( TaskStatus.RanToCompletion );
            task.Result.Should().Be( expectedResult );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnTaskThatRunsToCompletionWithDefaultResult_WhenSourceDisposes()
    {
        var source = new EventPublisher<int>();
        var task = source.ToTask( CancellationToken.None );

        source.Dispose();

        using ( new AssertionScope() )
        {
            task.Status.Should().Be( TaskStatus.RanToCompletion );
            task.Result.Should().Be( default );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnTaskThatRunsToCompletionWithLastEvent_WhenSourcePublishesEventsAndSubscriberDisposes()
    {
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var expectedResult = values[^1];
        var source = new EventPublisher<int>();
        var task = source.ToTask( CancellationToken.None );
        var subscriber = source.Subscribers.First();

        foreach ( var e in values )
            source.Publish( e );

        subscriber.Dispose();

        using ( new AssertionScope() )
        {
            task.Status.Should().Be( TaskStatus.RanToCompletion );
            task.Result.Should().Be( expectedResult );
        }
    }

    [Fact]
    public void ToTaskExtension_ShouldReturnTaskThatRunsToCompletionWithDefaultResult_WhenSubscriberDisposes()
    {
        var source = new EventPublisher<int>();
        var task = source.ToTask( CancellationToken.None );
        var subscriber = source.Subscribers.First();

        subscriber.Dispose();

        using ( new AssertionScope() )
        {
            task.Status.Should().Be( TaskStatus.RanToCompletion );
            task.Result.Should().Be( default );
        }
    }
}
