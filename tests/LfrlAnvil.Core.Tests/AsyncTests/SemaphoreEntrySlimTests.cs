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
        semaphore.CurrentCount.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Enter_ShouldThrow_WhenSemaphoreIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var action = Lambda.Of( () => SemaphoreEntrySlim.Enter( semaphore ) );

        action.Test( exc => exc.TestType().AssignableTo<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void TryEnter_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        var result = SemaphoreEntrySlim.TryEnter( semaphore, out var entered );

        Assertion.All(
                semaphore.CurrentCount.TestEquals( 0 ),
                result.Entered.TestEquals( entered ),
                entered.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var result = SemaphoreEntrySlim.TryEnter( semaphore, out var entered );

        Assertion.All(
                result.TestEquals( default ),
                result.Entered.TestEquals( entered ),
                entered.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task EnterAsync_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        _ = await SemaphoreEntrySlim.EnterAsync( semaphore );
        semaphore.CurrentCount.TestEquals( 0 ).Go();
    }

    [Fact]
    public void EnterAsync_ShouldThrow_WhenSemaphoreIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var action = Lambda.Of( async () => await SemaphoreEntrySlim.EnterAsync( semaphore ) );

        action.Test( exc => exc.TestType().AssignableTo<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public async Task TryEnterAsync_ShouldEnterSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        var result = await SemaphoreEntrySlim.TryEnterAsync( semaphore );

        Assertion.All(
                semaphore.CurrentCount.TestEquals( 0 ),
                result.Entered.TestTrue() )
            .Go();
    }

    [Fact]
    public async Task TryEnterAsync_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        semaphore.Dispose();

        var result = await SemaphoreEntrySlim.TryEnterAsync( semaphore );

        Assertion.All(
                result.TestEquals( default ),
                result.Entered.TestFalse() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseSemaphoreOnce()
    {
        var semaphore = new SemaphoreSlim( initialCount: 2 );
        var sut = SemaphoreEntrySlim.Enter( semaphore );

        sut.Dispose();

        semaphore.CurrentCount.TestEquals( 2 ).Go();
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDefault()
    {
        var sut = default( SemaphoreEntrySlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDisposedSemaphore()
    {
        var semaphore = new SemaphoreSlim( initialCount: 1 );
        var sut = SemaphoreEntrySlim.Enter( semaphore );
        semaphore.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
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

        Assertion.All(
                firstCount.TestEquals( 1 ),
                secondCount.TestEquals( 0 ),
                countAfterFirstDisposal.TestEquals( 1 ),
                countAfterSecondDisposal.TestEquals( 2 ) )
            .Go();
    }
}
