using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Requests;

public interface IAsyncValueTaskRequest<TRequest, TResult> : IRequest<TRequest, ValueTask<TResult>>
    where TRequest : IRequest<TRequest, ValueTask<TResult>>
{
    CancellationToken CancellationToken { get; }
}
