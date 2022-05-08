using System.Threading;

namespace LfrlAnvil.Requests.Tests
{
    public sealed class TestRequestClass : IRequest<TestRequestClass, int> { }

    public sealed class InvalidTestRequestClass : IRequest<TestRequestClass, int> { }

    public sealed class TestAsyncTaskRequestClass : IAsyncTaskRequest<TestAsyncTaskRequestClass, int>
    {
        public CancellationToken CancellationToken => CancellationToken.None;
    }

    public sealed class TestAsyncValueTaskRequestClass : IAsyncValueTaskRequest<TestAsyncValueTaskRequestClass, int>
    {
        public CancellationToken CancellationToken => CancellationToken.None;
    }

    public readonly struct TestRequestStruct : IRequest<TestRequestStruct, int> { }

    public readonly struct TestAsyncTaskRequestStruct : IAsyncTaskRequest<TestAsyncTaskRequestStruct, int>
    {
        public CancellationToken CancellationToken => CancellationToken.None;
    }

    public readonly struct TestAsyncValueTaskRequestStruct : IAsyncValueTaskRequest<TestAsyncValueTaskRequestStruct, int>
    {
        public CancellationToken CancellationToken => CancellationToken.None;
    }
}
