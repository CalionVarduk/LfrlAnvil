using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class UpgradeableReadLockSlimTests : TestsBase
{
    [Fact]
    public void Enter_ShouldAcquireUpgradeableReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = UpgradeableReadLockSlim.Enter( @lock );

        Assertion.All(
                @lock.IsReadLockHeld.TestFalse(),
                @lock.IsUpgradeableReadLockHeld.TestTrue(),
                @lock.IsWriteLockHeld.TestFalse() )
            .Go();
    }

    [Fact]
    public void Enter_ShouldThrow_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var action = Lambda.Of( () => UpgradeableReadLockSlim.Enter( @lock ) );

        action.Test( exc => exc.TestType().AssignableTo<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void TryEnter_ShouldAcquireUpgradeableReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = UpgradeableReadLockSlim.TryEnter( @lock, out var entered );

        Assertion.All(
                entered.TestTrue(),
                @lock.IsReadLockHeld.TestFalse(),
                @lock.IsUpgradeableReadLockHeld.TestTrue(),
                @lock.IsWriteLockHeld.TestFalse() )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var result = UpgradeableReadLockSlim.TryEnter( @lock, out var entered );

        Assertion.All(
                result.TestEquals( default ),
                entered.TestFalse() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseUpgradeableReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = UpgradeableReadLockSlim.Enter( @lock );

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
        var sut = default( UpgradeableReadLockSlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Upgrade_ShouldAcquireWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = UpgradeableReadLockSlim.Enter( @lock );

        _ = sut.Upgrade();

        Assertion.All(
                @lock.IsReadLockHeld.TestFalse(),
                @lock.IsUpgradeableReadLockHeld.TestTrue(),
                @lock.IsWriteLockHeld.TestTrue() )
            .Go();
    }

    [Fact]
    public void Upgrade_ShouldNotThrowForDefault()
    {
        var sut = default( UpgradeableReadLockSlim );
        var result = sut.Upgrade();
        result.TestEquals( default ).Go();
    }

    [Fact]
    public void MultipleEntries_ShouldBehaveCorrectly()
    {
        var @lock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );

        var first = UpgradeableReadLockSlim.Enter( @lock );
        var hasFirstLock = @lock.IsUpgradeableReadLockHeld;
        var second = UpgradeableReadLockSlim.Enter( @lock );
        var hasSecondLock = @lock.IsUpgradeableReadLockHeld;

        second.Dispose();
        var hasLockAfterFirstDisposal = @lock.IsUpgradeableReadLockHeld;
        first.Dispose();
        var hasLockAfterSecondDisposal = @lock.IsUpgradeableReadLockHeld;

        Assertion.All(
                hasFirstLock.TestTrue(),
                hasSecondLock.TestTrue(),
                hasLockAfterFirstDisposal.TestTrue(),
                hasLockAfterSecondDisposal.TestFalse() )
            .Go();
    }
}
