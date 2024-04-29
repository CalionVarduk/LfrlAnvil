using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Requests.Exceptions;

namespace LfrlAnvil.Requests;

/// <summary>
/// Represents a dispatcher of generic <see cref="IRequest{TRequest,TResult}"/> instances.
/// </summary>
public interface IRequestDispatcher
{
    /// <summary>
    /// Attempts to dispatch the provided <see cref="IRequest{TRequest,TResult}"/> instance.
    /// </summary>
    /// <param name="request">Request to handle.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns a result of the provided <paramref name="request"/> if it was handled.
    /// </param>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResult">Request's result type.</typeparam>
    /// <returns><b>true</b> when the provided <paramref name="request"/> was handled, otherwise <b>false</b>.</returns>
    /// <exception cref="InvalidRequestTypeException">
    /// When the provided <paramref name="request"/> is not of <typeparamref name="TRequest"/> type.
    /// </exception>
    bool TryDispatch<TRequest, TResult>(IRequest<TRequest, TResult> request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : class, IRequest<TRequest, TResult>;

    /// <summary>
    /// Attempts to dispatch the provided <see cref="IRequest{TRequest,TResult}"/> instance.
    /// </summary>
    /// <param name="request">Request to handle.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns a result of the provided <paramref name="request"/> if it was handled.
    /// </param>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResult">Request's result type.</typeparam>
    /// <returns><b>true</b> when the provided <paramref name="request"/> was handled, otherwise <b>false</b>.</returns>
    bool TryDispatch<TRequest, TResult>(TRequest request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : struct, IRequest<TRequest, TResult>;
}
