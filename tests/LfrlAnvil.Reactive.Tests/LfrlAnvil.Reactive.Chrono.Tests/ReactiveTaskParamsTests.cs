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

        Assertion.All(
                sut.Invocation.InvocationId.TestEquals( invocationId ),
                sut.Invocation.OriginalTimestamp.TestEquals( originalTimestamp ),
                sut.Invocation.InvocationTimestamp.TestEquals( invocationTimestamp ),
                sut.ElapsedTime.TestEquals( elapsedTime ),
                sut.Exception.TestNull(),
                sut.CancellationReason.TestNull(),
                sut.IsFailed.TestFalse(),
                sut.IsSuccessful.TestTrue() )
            .Go();
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

        Assertion.All(
                sut.Invocation.InvocationId.TestEquals( invocationId ),
                sut.Invocation.OriginalTimestamp.TestEquals( originalTimestamp ),
                sut.Invocation.InvocationTimestamp.TestEquals( invocationTimestamp ),
                sut.ElapsedTime.TestEquals( elapsedTime ),
                sut.Exception.TestRefEquals( exception ),
                sut.CancellationReason.TestNull(),
                sut.IsFailed.TestTrue(),
                sut.IsSuccessful.TestFalse() )
            .Go();
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

        Assertion.All(
                sut.Invocation.InvocationId.TestEquals( invocationId ),
                sut.Invocation.OriginalTimestamp.TestEquals( originalTimestamp ),
                sut.Invocation.InvocationTimestamp.TestEquals( invocationTimestamp ),
                sut.ElapsedTime.TestEquals( elapsedTime ),
                sut.Exception.TestNull(),
                sut.CancellationReason.TestEquals( reason ),
                sut.IsFailed.TestFalse(),
                sut.IsSuccessful.TestFalse() )
            .Go();
    }
}
