using System.Threading;
using FluentAssertions;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.AsyncTests.DisposableLockTests
{
    public class DisposableLockTests : TestsBase
    {
        [Fact]
        public void Default_ShouldNotAcquireAnyLocksAndNotThrowOnDispose()
        {
            var sut = default( DisposableLock );
            var action = Lambda.Of( () => sut.Dispose() );
            action.Should().NotThrow();
        }

        [Fact]
        public void Ctor_ShouldAcquireLockOnTheParameter()
        {
            var sync = new object();
            var _ = new DisposableLock( sync );

            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeTrue();
        }

        [Fact]
        public void Dispose_ShouldReleaseLockOnTheParameter()
        {
            var sync = new object();
            var sut = new DisposableLock( sync );

            sut.Dispose();
            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeFalse();
        }
    }
}
