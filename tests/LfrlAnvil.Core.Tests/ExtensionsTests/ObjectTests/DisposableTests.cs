using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests;

public class DisposableTests : TestsBase
{
    [Fact]
    public void TryDispose_ShouldReturnValid_WhenDisposalDoesNotThrow()
    {
        var sut = new Valid();
        var result = sut.TryDispose();
        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            sut.DisposeCalled.Should().BeTrue();
        }
    }

    [Fact]
    public void TryDispose_ShouldReturnInvalid_WhenDisposalThrows()
    {
        var sut = new Invalid();
        var result = sut.TryDispose();
        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<NotSupportedException>();
            sut.DisposeCalled.Should().BeTrue();
        }
    }

    private sealed class Valid : IDisposable
    {
        public bool DisposeCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled = true;
        }
    }

    private sealed class Invalid : IDisposable
    {
        public bool DisposeCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled = true;
            throw new NotSupportedException();
        }
    }
}
