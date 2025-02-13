using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class ManualResetValueTaskSourceTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Ctor_ShouldCreateCorrectResult(bool runContinuationsAsynchronously)
    {
        var sut = new ManualResetValueTaskSource<int>( runContinuationsAsynchronously );

        Assertion.All(
                sut.RunContinuationsAsynchronously.TestEquals( runContinuationsAsynchronously ),
                sut.Status.TestEquals( ValueTaskSourceStatus.Pending ) )
            .Go();
    }

    [Fact]
    public async Task SetResult_ShouldCompleteCurrentOperation()
    {
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetResult( 123 );
            } );

        var result = await sut.GetTask();

        Assertion.All(
                result.TestEquals( 123 ),
                sut.Status.TestEquals( ValueTaskSourceStatus.Succeeded ) )
            .Go();
    }

    [Fact]
    public void SetException_ShouldCompleteCurrentOperation()
    {
        var exception = new Exception( "foo" );
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetException( exception );
            } );

        var action = Lambda.Of( async () => await sut.GetTask() );

        action.Test(
                exc => Assertion.All(
                    exc.TestRefEquals( exception ),
                    sut.Status.TestEquals( ValueTaskSourceStatus.Faulted ) ) )
            .Go();
    }

    [Fact]
    public void SetCancelled_ShouldCompleteCurrentOperation()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        cancellationTokenSource.Cancel();

        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetCancelled( token );
            } );

        var action = Lambda.Of( async () => await sut.GetTask() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>( e => e.CancellationToken.TestEquals( token ) ),
                    sut.Status.TestEquals( ValueTaskSourceStatus.Canceled ) ) )
            .Go();
    }

    [Fact]
    public async Task Reset_ShouldStartNextOperation()
    {
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetResult( 123 );
            } );

        _ = await sut.GetTask();
        sut.Reset();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetResult( 456 );
            } );

        var result = await sut.GetTask();

        Assertion.All(
                result.TestEquals( 456 ),
                sut.Status.TestEquals( ValueTaskSourceStatus.Succeeded ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutVersionAndStatus()
    {
        var sut = new ManualResetValueTaskSource<int>();
        var result = sut.ToString();
        result.TestEquals( "Version: 0, Status: Pending" ).Go();
    }
}
