using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Requests;

/// <summary>
/// Represents an asynchronous single generic request. This is the <see cref="ValueTask{T}"/> version.
/// </summary>
/// <typeparam name="TRequest">Request type. Use the "Curiously Recurring Template Pattern" (CRTP) approach.</typeparam>
/// <typeparam name="TResult">Request's result type.</typeparam>
public interface IAsyncValueTaskRequest<TRequest, TResult> : IRequest<TRequest, ValueTask<TResult>>
    where TRequest : IRequest<TRequest, ValueTask<TResult>>
{
    /// <summary>
    /// Request's cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }
}
