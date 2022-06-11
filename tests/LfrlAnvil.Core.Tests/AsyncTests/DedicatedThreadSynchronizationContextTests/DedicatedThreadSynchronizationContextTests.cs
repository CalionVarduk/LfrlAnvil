using System.Globalization;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Async;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.AsyncTests.DedicatedThreadSynchronizationContextTests
{
    public class DedicatedThreadSynchronizationContextTests : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateWithDefaultParameters()
        {
            var sut = new DedicatedThreadSynchronizationContext();

            using ( new AssertionScope() )
            {
                sut.IsActive.Should().BeTrue();
                sut.ThreadCulture.Should().NotBeNull();
                sut.ThreadUICulture.Should().NotBeNull();
                sut.ThreadName.Should().BeNull();
                sut.ThreadPriority.Should().Be( ThreadPriority.Normal );
                sut.ThreadId.Should().BeGreaterThan( 0 );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateWithCustomParameters()
        {
            var @params = new ThreadParams
            {
                Culture = Fixture.Create<CultureInfo>(),
                UICulture = Fixture.Create<CultureInfo>(),
                Name = Fixture.Create<string>(),
                Priority = Fixture.Create<ThreadPriority>()
            };

            var sut = new DedicatedThreadSynchronizationContext( @params );

            using ( new AssertionScope() )
            {
                sut.IsActive.Should().BeTrue();
                sut.ThreadCulture.Should().BeSameAs( @params.Culture );
                sut.ThreadUICulture.Should().BeSameAs( @params.UICulture );
                sut.ThreadName.Should().Be( @params.Name );
                sut.ThreadPriority.Should().Be( @params.Priority );
                sut.ThreadId.Should().BeGreaterThan( 0 );
            }
        }

        [Fact]
        public void Send_ShouldInvokeCallbackSynchronouslyOnTheUnderlyingThread()
        {
            object? capturedState = null;
            var capturedThreadId = -1;
            var state = new object();

            var sut = new DedicatedThreadSynchronizationContext();

            sut.Send(
                s =>
                {
                    capturedState = s;
                    capturedThreadId = Thread.CurrentThread.ManagedThreadId;
                },
                state );

            using ( new AssertionScope() )
            {
                capturedState.Should().BeSameAs( state );
                capturedThreadId.Should().Be( sut.ThreadId );
            }
        }

        [Fact]
        public void Post_ShouldInvokeCallbackAsynchronouslyOnTheUnderlyingThread()
        {
            object? capturedState = null;
            var capturedThreadId = -1;
            var state = new object();

            var sut = new DedicatedThreadSynchronizationContext();

            var reset = new ManualResetEvent( false );

            sut.Post(
                s =>
                {
                    capturedState = s;
                    capturedThreadId = Thread.CurrentThread.ManagedThreadId;
                    reset.Set();
                },
                state );

            reset.WaitOne();

            using ( new AssertionScope() )
            {
                capturedState.Should().BeSameAs( state );
                capturedThreadId.Should().Be( sut.ThreadId );
            }
        }
    }
}
