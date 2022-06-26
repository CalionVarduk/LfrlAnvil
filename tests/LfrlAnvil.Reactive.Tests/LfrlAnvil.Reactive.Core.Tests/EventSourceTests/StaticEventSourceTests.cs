using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventSourceTests;

public class StaticEventSourceTests : TestsBase
{
    [Fact]
    public void Disposed_ShouldReturnDisposedSource()
    {
        var sut = EventSource<int>.Disposed;
        sut.IsDisposed.Should().BeTrue();
    }
}
