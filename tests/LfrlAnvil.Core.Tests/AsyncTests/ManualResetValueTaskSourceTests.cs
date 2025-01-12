using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class ManualResetValueTaskSourceTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Ctor_ShouldCreateCorrectResult(bool runContinuationsAsynchronously)
    {
        var sut = new ManualResetValueTaskSource<int>( runContinuationsAsynchronously );

        using ( new AssertionScope() )
        {
            sut.RunContinuationsAsynchronously.Should().Be( runContinuationsAsynchronously );
            sut.Status.Should().Be( ValueTaskSourceStatus.Pending );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( 123 );
            sut.Status.Should().Be( ValueTaskSourceStatus.Succeeded );
        }
    }

    [Fact]
    public async Task SetException_ShouldCompleteCurrentOperation()
    {
        var exception = new Exception( "foo" );
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetException( exception );
            } );

        Exception? caughtException = null;
        try
        {
            _ = await sut.GetTask();
        }
        catch ( Exception exc )
        {
            caughtException = exc;
        }

        using ( new AssertionScope() )
        {
            caughtException.Should().BeSameAs( exception );
            sut.Status.Should().Be( ValueTaskSourceStatus.Faulted );
        }
    }

    [Fact]
    public async Task SetCancelled_ShouldCompleteCurrentOperation()
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

        Exception? caughtException = null;
        try
        {
            _ = await sut.GetTask();
        }
        catch ( Exception exc )
        {
            caughtException = exc;
        }

        using ( new AssertionScope() )
        {
            caughtException.Should().BeOfType<OperationCanceledException>();
            ((caughtException as OperationCanceledException)?.CancellationToken).Should().Be( token );
            sut.Status.Should().Be( ValueTaskSourceStatus.Canceled );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( 456 );
            sut.Status.Should().Be( ValueTaskSourceStatus.Succeeded );
        }
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutVersionAndStatus()
    {
        var sut = new ManualResetValueTaskSource<int>();
        var result = sut.ToString();
        result.Should().Be( "Version: 0, Status: Pending" );
    }
}
