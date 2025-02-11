using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class WriteLockSlimTests : TestsBase
{
    [Fact]
    public void Enter_ShouldAcquireWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = WriteLockSlim.Enter( @lock );

        Assertion.All(
                @lock.IsReadLockHeld.TestFalse(),
                @lock.IsUpgradeableReadLockHeld.TestFalse(),
                @lock.IsWriteLockHeld.TestTrue() )
            .Go();
    }

    [Fact]
    public void Enter_ShouldThrow_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var action = Lambda.Of( () => WriteLockSlim.Enter( @lock ) );

        action.Test( exc => exc.TestType().AssignableTo<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void TryEnter_ShouldAcquireWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = WriteLockSlim.TryEnter( @lock, out var entered );

        Assertion.All(
                entered.TestTrue(),
                @lock.IsReadLockHeld.TestFalse(),
                @lock.IsUpgradeableReadLockHeld.TestFalse(),
                @lock.IsWriteLockHeld.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var result = WriteLockSlim.TryEnter( @lock, out var entered );

        Assertion.All(
                result.TestEquals( default ),
                entered.TestFalse() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = WriteLockSlim.Enter( @lock );

        sut.Dispose();

        Assertion.All(
                @lock.IsReadLockHeld.TestFalse(),
                @lock.IsUpgradeableReadLockHeld.TestFalse(),
                @lock.IsWriteLockHeld.TestFalse() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDefault()
    {
        var sut = default( WriteLockSlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void MultipleEntries_ShouldBehaveCorrectly()
    {
        var @lock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );

        var first = WriteLockSlim.Enter( @lock );
        var hasFirstLock = @lock.IsWriteLockHeld;
        var second = WriteLockSlim.Enter( @lock );
        var hasSecondLock = @lock.IsWriteLockHeld;

        second.Dispose();
        var hasLockAfterFirstDisposal = @lock.IsWriteLockHeld;
        first.Dispose();
        var hasLockAfterSecondDisposal = @lock.IsWriteLockHeld;

        Assertion.All(
                hasFirstLock.TestTrue(),
                hasSecondLock.TestTrue(),
                hasLockAfterFirstDisposal.TestTrue(),
                hasLockAfterSecondDisposal.TestFalse() )
            .Go();
    }
}
