using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class ReadLockSlimTests : TestsBase
{
    [Fact]
    public void Enter_ShouldAcquireReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = ReadLockSlim.Enter( @lock );

        Assertion.All(
                @lock.IsReadLockHeld.TestTrue(),
                @lock.IsUpgradeableReadLockHeld.TestFalse(),
                @lock.IsWriteLockHeld.TestFalse() )
            .Go();
    }

    [Fact]
    public void Enter_ShouldThrow_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var action = Lambda.Of( () => ReadLockSlim.Enter( @lock ) );

        action.Test( exc => exc.TestType().AssignableTo<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void TryEnter_ShouldAcquireReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = ReadLockSlim.TryEnter( @lock, out var entered );

        Assertion.All(
                entered.TestTrue(),
                @lock.IsReadLockHeld.TestTrue(),
                @lock.IsUpgradeableReadLockHeld.TestFalse(),
                @lock.IsWriteLockHeld.TestFalse() )
            .Go();
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var result = ReadLockSlim.TryEnter( @lock, out var entered );

        Assertion.All(
                result.TestEquals( default ),
                entered.TestFalse() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = ReadLockSlim.Enter( @lock );

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
        var sut = default( ReadLockSlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void MultipleEntries_ShouldBehaveCorrectly()
    {
        var @lock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );

        var first = ReadLockSlim.Enter( @lock );
        var hasFirstLock = @lock.IsReadLockHeld;
        var second = ReadLockSlim.Enter( @lock );
        var hasSecondLock = @lock.IsReadLockHeld;

        second.Dispose();
        var hasLockAfterFirstDisposal = @lock.IsReadLockHeld;
        first.Dispose();
        var hasLockAfterSecondDisposal = @lock.IsReadLockHeld;

        Assertion.All(
                hasFirstLock.TestTrue(),
                hasSecondLock.TestTrue(),
                hasLockAfterFirstDisposal.TestTrue(),
                hasLockAfterSecondDisposal.TestFalse() )
            .Go();
    }
}
