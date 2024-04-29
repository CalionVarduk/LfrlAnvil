namespace LfrlAnvil.Requests;

/// <summary>
/// Represents a handler for a generic <see cref="IRequest{TRequest,TResult}"/>.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResult">Request's result type.</typeparam>
public interface IRequestHandler<in TRequest, out TResult>
    where TRequest : IRequest<TRequest, TResult>
{
    /// <summary>
    /// Handles the provided <paramref name="request"/> and returns its result.
    /// </summary>
    /// <param name="request">Request to handle.</param>
    /// <returns>Request's result.</returns>
    TResult Handle(TRequest request);
}
