namespace LfrlAnvil.Requests;

/// <summary>
/// Represents a factory of generic <see cref="IRequestHandler{TRequest,TResult}"/> instances.
/// </summary>
public interface IRequestHandlerFactory
{
    /// <summary>
    /// Attempts to create an <see cref="IRequestHandler{TRequest,TResult}"/> instance.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResult">Request's result type.</typeparam>
    /// <returns><see cref="IRequestHandler{TRequest,TResult}"/> instance or null when instance could not be created.</returns>
    IRequestHandler<TRequest, TResult>? TryCreate<TRequest, TResult>()
        where TRequest : IRequest<TRequest, TResult>;
}
