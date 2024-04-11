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

        using ( new AssertionScope() )
        {
            @lock.IsReadLockHeld.Should().BeFalse();
            @lock.IsUpgradeableReadLockHeld.Should().BeFalse();
            @lock.IsWriteLockHeld.Should().BeTrue();
        }
    }

    [Fact]
    public void Enter_ShouldThrow_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var action = Lambda.Of( () => WriteLockSlim.Enter( @lock ) );

        action.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void TryEnter_ShouldAcquireWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = WriteLockSlim.TryEnter( @lock, out var entered );

        using ( new AssertionScope() )
        {
            entered.Should().BeTrue();
            @lock.IsReadLockHeld.Should().BeFalse();
            @lock.IsUpgradeableReadLockHeld.Should().BeFalse();
            @lock.IsWriteLockHeld.Should().BeTrue();
        }
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var result = WriteLockSlim.TryEnter( @lock, out var entered );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( WriteLockSlim ) );
            entered.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldReleaseWriteLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = WriteLockSlim.Enter( @lock );

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
        var sut = default( WriteLockSlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
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

        using ( new AssertionScope() )
        {
            hasFirstLock.Should().BeTrue();
            hasSecondLock.Should().BeTrue();
            hasLockAfterFirstDisposal.Should().BeTrue();
            hasLockAfterSecondDisposal.Should().BeFalse();
        }
    }
}
