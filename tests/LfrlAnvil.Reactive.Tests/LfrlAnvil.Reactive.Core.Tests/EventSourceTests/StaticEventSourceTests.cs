namespace LfrlAnvil.Reactive.Tests.EventSourceTests;

public class StaticEventSourceTests : TestsBase
{
    [Fact]
    public void Disposed_ShouldReturnDisposedSource()
    {
        var sut = EventSource.Disposed<int>();
        sut.IsDisposed.Should().BeTrue();
    }
}
