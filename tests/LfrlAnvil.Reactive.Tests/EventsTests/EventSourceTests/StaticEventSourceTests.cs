using FluentAssertions;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.EventSourceTests
{
    public class StaticEventSourceTests : TestsBase
    {
        [Fact]
        public void Disposed_ShouldReturnDisposedSource()
        {
            var sut = EventSource<int>.Disposed;
            sut.IsDisposed.Should().BeTrue();
        }
    }
}
