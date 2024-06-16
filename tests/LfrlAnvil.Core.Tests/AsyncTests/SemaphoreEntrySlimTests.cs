using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class SemaphoreEntrySlimTests : TestsBase
{
    [Fact]
    public void Enter_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        _ = SemaphoreEntrySlim.Enter( semaphore );
        semaphore.CurrentCount.Should().Be( 0 );
    }

    [Fact]
    public void Enter_ShouldThrow_WhenSemaphoreIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var action = Lambda.Of( () => SemaphoreEntrySlim.Enter( semaphore ) );

        action.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void TryEnter_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        var result = SemaphoreEntrySlim.TryEnter( semaphore, out var entered );

        using ( new AssertionScope() )
        {
            semaphore.CurrentCount.Should().Be( 0 );
            result.Entered.Should().Be( entered );
            entered.Should().BeTrue();
        }
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var result = SemaphoreEntrySlim.TryEnter( semaphore, out var entered );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( SemaphoreEntrySlim ) );
            result.Entered.Should().Be( entered );
            entered.Should().BeFalse();
        }
    }

    [Fact]
    public async Task EnterAsync_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        _ = await SemaphoreEntrySlim.EnterAsync( semaphore );
        semaphore.CurrentCount.Should().Be( 0 );
    }

    [Fact]
    public async Task EnterAsync_ShouldThrow_WhenSemaphoreIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        Exception? exception = null;
        try
        {
            await SemaphoreEntrySlim.EnterAsync( semaphore );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        exception.Should().BeOfType<ObjectDisposedException>();
    }

    [Fact]
    public async Task TryEnterAsync_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        var result = await SemaphoreEntrySlim.TryEnterAsync( semaphore );

        using ( new AssertionScope() )
        {
            semaphore.CurrentCount.Should().Be( 0 );
            result.Entered.Should().BeTrue();
        }
    }

    [Fact]
    public async Task TryEnterAsync_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var result = await SemaphoreEntrySlim.TryEnterAsync( semaphore );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( SemaphoreEntrySlim ) );
            result.Entered.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldReleaseSemaphoreOnce()
    {
        var semaphore = new SemaphoreSlim( initialCount: 2 );
        var sut = SemaphoreEntrySlim.Enter( semaphore );

        sut.Dispose();

        semaphore.CurrentCount.Should().Be( 2 );
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDefault()
    {
        var sut = default( SemaphoreEntrySlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDisposedSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        var sut = SemaphoreEntrySlim.Enter( semaphore );
        semaphore.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public void MultipleEntries_ShouldBehaveCorrectly()
    {
        var semaphore = new SemaphoreSlim( initialCount: 2 );

        var first = SemaphoreEntrySlim.Enter( semaphore );
        var firstCount = semaphore.CurrentCount;
        var second = SemaphoreEntrySlim.Enter( semaphore );
        var secondCount = semaphore.CurrentCount;

        second.Dispose();
        var countAfterFirstDisposal = semaphore.CurrentCount;
        first.Dispose();
        var countAfterSecondDisposal = semaphore.CurrentCount;

        using ( new AssertionScope() )
        {
            firstCount.Should().Be( 1 );
            secondCount.Should().Be( 0 );
            countAfterFirstDisposal.Should().Be( 1 );
            countAfterSecondDisposal.Should().Be( 2 );
        }
    }
}
