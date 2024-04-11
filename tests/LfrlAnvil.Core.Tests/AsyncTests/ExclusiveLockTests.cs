using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class ExclusiveLockTests : TestsBase
{
    [Fact]
    public void Enter_ShouldAcquireLockOnTheParameter()
    {
        var sync = new object();
        _ = ExclusiveLock.Enter( sync );

        var hasLock = Monitor.IsEntered( sync );

        hasLock.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldReleaseLockOnTheParameter()
    {
        var sync = new object();
        var sut = ExclusiveLock.Enter( sync );

        sut.Dispose();
        var hasLock = Monitor.IsEntered( sync );

        hasLock.Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDefault()
    {
        var sut = default( ExclusiveLock );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
    }

    [Fact]
    public void MultipleEntries_ShouldBehaveCorrectly()
    {
        var sync = new object();

        var first = ExclusiveLock.Enter( sync );
        var hasFirstLock = Monitor.IsEntered( sync );
        var second = ExclusiveLock.Enter( sync );
        var hasSecondLock = Monitor.IsEntered( sync );

        second.Dispose();
        var hasLockAfterFirstDisposal = Monitor.IsEntered( sync );
        first.Dispose();
        var hasLockAfterSecondDisposal = Monitor.IsEntered( sync );

        using ( new AssertionScope() )
        {
            hasFirstLock.Should().BeTrue();
            hasSecondLock.Should().BeTrue();
            hasLockAfterFirstDisposal.Should().BeTrue();
            hasLockAfterSecondDisposal.Should().BeFalse();
        }
    }
}
