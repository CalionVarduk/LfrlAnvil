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

        using ( new AssertionScope() )
        {
            @lock.IsReadLockHeld.Should().BeFalse();
            @lock.IsUpgradeableReadLockHeld.Should().BeTrue();
            @lock.IsWriteLockHeld.Should().BeFalse();
        }
    }

    [Fact]
    public void Enter_ShouldThrow_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var action = Lambda.Of( () => UpgradeableReadLockSlim.Enter( @lock ) );

        action.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void TryEnter_ShouldAcquireUpgradeableReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = UpgradeableReadLockSlim.TryEnter( @lock, out var entered );

        using ( new AssertionScope() )
        {
            entered.Should().BeTrue();
            @lock.IsReadLockHeld.Should().BeFalse();
            @lock.IsUpgradeableReadLockHeld.Should().BeTrue();
            @lock.IsWriteLockHeld.Should().BeFalse();
        }
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var result = UpgradeableReadLockSlim.TryEnter( @lock, out var entered );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( UpgradeableReadLockSlim ) );
            entered.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldReleaseUpgradeableReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = UpgradeableReadLockSlim.Enter( @lock );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            @lock.IsReadLockHeld.Should().BeFalse();
            @lock.IsUpgradeableReadLockHeld.Should().BeFalse();
            @lock.IsWriteLockHeld.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDefault()
    {
        var sut = default( UpgradeableReadLockSlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
    }

    [Fact]
    public void Upgrade_ShouldAcquireWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = UpgradeableReadLockSlim.Enter( @lock );

        _ = sut.Upgrade();

        using ( new AssertionScope() )
        {
            @lock.IsReadLockHeld.Should().BeFalse();
            @lock.IsUpgradeableReadLockHeld.Should().BeTrue();
            @lock.IsWriteLockHeld.Should().BeTrue();
        }
    }

    [Fact]
    public void Upgrade_ShouldNotThrowForDefault()
    {
        var sut = default( UpgradeableReadLockSlim );
        var result = sut.Upgrade();
        result.Should().Be( default( WriteLockSlim ) );
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

        using ( new AssertionScope() )
        {
            hasFirstLock.Should().BeTrue();
            hasSecondLock.Should().BeTrue();
            hasLockAfterFirstDisposal.Should().BeTrue();
            hasLockAfterSecondDisposal.Should().BeFalse();
        }
    }
}
