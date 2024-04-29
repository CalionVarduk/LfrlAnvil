using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Requests;

/// <summary>
/// Represents an asynchronous single generic request. This is the <see cref="Task{T}"/> version.
/// </summary>
/// <typeparam name="TRequest">Request type. Use the "Curiously Recurring Template Pattern" (CRTP) approach.</typeparam>
/// <typeparam name="TResult">Request's result type.</typeparam>
public interface IAsyncTaskRequest<TRequest, TResult> : IRequest<TRequest, Task<TResult>>
    where TRequest : IRequest<TRequest, Task<TResult>>
{
    /// <summary>
    /// Request's cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }
}
