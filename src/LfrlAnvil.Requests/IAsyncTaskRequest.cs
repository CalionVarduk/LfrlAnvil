using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Requests
{
    public interface IAsyncTaskRequest<TRequest, TResult> : IRequest<TRequest, Task<TResult>>
        where TRequest : IRequest<TRequest, Task<TResult>>
    {
        CancellationToken CancellationToken { get; }
    }
}
