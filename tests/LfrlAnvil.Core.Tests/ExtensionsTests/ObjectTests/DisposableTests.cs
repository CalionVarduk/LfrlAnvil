using System.Threading.Tasks;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests;

public class DisposableTests : TestsBase
{
    [Fact]
    public void TryDispose_ShouldReturnValid_WhenDisposalDoesNotThrow()
    {
        var sut = new Valid();
        var result = sut.TryDispose();
        Assertion.All(
                result.Exception.TestNull(),
                sut.DisposeCalled.TestTrue() )
            .Go();
    }

    [Fact]
    public void TryDispose_ShouldReturnInvalid_WhenDisposalThrows()
    {
        var sut = new Invalid();
        var result = sut.TryDispose();
        Assertion.All(
                result.Exception.TestType().AssignableTo<NotSupportedException>(),
                sut.DisposeCalled.TestTrue() )
            .Go();
    }

    [Fact]
    public async Task TryDisposeAsync_ShouldReturnValid_WhenDisposalDoesNotThrow()
    {
        var sut = new Valid();
        var result = await sut.TryDisposeAsync();
        Assertion.All(
                result.Exception.TestNull(),
                sut.DisposeCalled.TestTrue() )
            .Go();
    }

    [Fact]
    public async Task TryDisposeAsync_ShouldReturnInvalid_WhenDisposalThrows()
    {
        var sut = new Invalid();
        var result = await sut.TryDisposeAsync();
        Assertion.All(
                result.Exception.TestType().AssignableTo<NotSupportedException>(),
                sut.DisposeCalled.TestTrue() )
            .Go();
    }

    private sealed class Valid : IDisposable, IAsyncDisposable
    {
        public bool DisposeCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled = true;
        }

        public ValueTask DisposeAsync()
        {
            DisposeCalled = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class Invalid : IDisposable, IAsyncDisposable
    {
        public bool DisposeCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled = true;
            throw new NotSupportedException();
        }

        public ValueTask DisposeAsync()
        {
            DisposeCalled = true;
            return ValueTask.FromException( new NotSupportedException() );
        }
    }
}
