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

        using ( new AssertionScope() )
        {
            @lock.IsReadLockHeld.Should().BeTrue();
            @lock.IsUpgradeableReadLockHeld.Should().BeFalse();
            @lock.IsWriteLockHeld.Should().BeFalse();
        }
    }

    [Fact]
    public void Enter_ShouldThrow_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var action = Lambda.Of( () => ReadLockSlim.Enter( @lock ) );

        action.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void TryEnter_ShouldAcquireReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        _ = ReadLockSlim.TryEnter( @lock, out var entered );

        using ( new AssertionScope() )
        {
            entered.Should().BeTrue();
            @lock.IsReadLockHeld.Should().BeTrue();
            @lock.IsUpgradeableReadLockHeld.Should().BeFalse();
            @lock.IsWriteLockHeld.Should().BeFalse();
        }
    }

    [Fact]
    public void TryEnter_ShouldReturnDefault_WhenLockIsDisposed()
    {
        var @lock = new ReaderWriterLockSlim();
        @lock.Dispose();

        var result = ReadLockSlim.TryEnter( @lock, out var entered );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( ReadLockSlim ) );
            entered.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldReleaseReadLock()
    {
        var @lock = new ReaderWriterLockSlim();
        var sut = ReadLockSlim.Enter( @lock );

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
        var sut = default( ReadLockSlim );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
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

        using ( new AssertionScope() )
        {
            hasFirstLock.Should().BeTrue();
            hasSecondLock.Should().BeTrue();
            hasLockAfterFirstDisposal.Should().BeTrue();
            hasLockAfterSecondDisposal.Should().BeFalse();
        }
    }
}
