using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class ReactiveTaskParamsTests : TestsBase
{
    [Fact]
    public void CompletionParams_WithoutExceptionAndNotCancelled()
    {
        var invocationId = Fixture.Create<long>();
        var originalTimestamp = new Timestamp( 123 );
        var invocationTimestamp = new Timestamp( 456 );
        var elapsedTime = new Duration( 42 );

        var sut = new ReactiveTaskCompletionParams(
            new ReactiveTaskInvocationParams( invocationId, originalTimestamp, invocationTimestamp ),
            elapsedTime,
            Exception: null,
            CancellationReason: null );

        using ( new AssertionScope() )
        {
            sut.Invocation.InvocationId.Should().Be( invocationId );
            sut.Invocation.OriginalTimestamp.Should().Be( originalTimestamp );
            sut.Invocation.InvocationTimestamp.Should().Be( invocationTimestamp );
            sut.ElapsedTime.Should().Be( elapsedTime );
            sut.Exception.Should().BeNull();
            sut.CancellationReason.Should().BeNull();
            sut.IsFailed.Should().BeFalse();
            sut.IsSuccessful.Should().BeTrue();
        }
    }

    [Fact]
    public void CompletionParams_WithException()
    {
        var invocationId = Fixture.Create<long>();
        var originalTimestamp = new Timestamp( 123 );
        var invocationTimestamp = new Timestamp( 456 );
        var elapsedTime = new Duration( 42 );
        var exception = new Exception();

        var sut = new ReactiveTaskCompletionParams(
            new ReactiveTaskInvocationParams( invocationId, originalTimestamp, invocationTimestamp ),
            elapsedTime,
            Exception: exception,
            CancellationReason: null );

        using ( new AssertionScope() )
        {
            sut.Invocation.InvocationId.Should().Be( invocationId );
            sut.Invocation.OriginalTimestamp.Should().Be( originalTimestamp );
            sut.Invocation.InvocationTimestamp.Should().Be( invocationTimestamp );
            sut.ElapsedTime.Should().Be( elapsedTime );
            sut.Exception.Should().BeSameAs( exception );
            sut.CancellationReason.Should().BeNull();
            sut.IsFailed.Should().BeTrue();
            sut.IsSuccessful.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( TaskCancellationReason.CancellationRequested )]
    [InlineData( TaskCancellationReason.MaxQueueSizeLimit )]
    [InlineData( TaskCancellationReason.TaskDisposed )]
    public void CompletionParams_Cancelled(TaskCancellationReason reason)
    {
        var invocationId = Fixture.Create<long>();
        var originalTimestamp = new Timestamp( 123 );
        var invocationTimestamp = new Timestamp( 456 );
        var elapsedTime = new Duration( 42 );

        var sut = new ReactiveTaskCompletionParams(
            new ReactiveTaskInvocationParams( invocationId, originalTimestamp, invocationTimestamp ),
            elapsedTime,
            Exception: null,
            CancellationReason: reason );

        using ( new AssertionScope() )
        {
            sut.Invocation.InvocationId.Should().Be( invocationId );
            sut.Invocation.OriginalTimestamp.Should().Be( originalTimestamp );
            sut.Invocation.InvocationTimestamp.Should().Be( invocationTimestamp );
            sut.ElapsedTime.Should().Be( elapsedTime );
            sut.Exception.Should().BeNull();
            sut.CancellationReason.Should().Be( reason );
            sut.IsFailed.Should().BeFalse();
            sut.IsSuccessful.Should().BeFalse();
        }
    }
}
