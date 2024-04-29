using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using LfrlAnvil.Requests.Exceptions;

namespace LfrlAnvil.Requests;

/// <summary>
/// Contains request extension methods.
/// </summary>
public static class RequestExtensions
{
    /// <summary>
    /// Dispatches the provided <see cref="IRequest{TRequest,TResult}"/> instance.
    /// </summary>
    /// <param name="dispatcher">Source dispatcher.</param>
    /// <param name="request">Request to handle.</param>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResult">Request's result type.</typeparam>
    /// <returns>Result of the provided <paramref name="request"/>.</returns>
    /// <exception cref="InvalidRequestTypeException">
    /// When the provided <paramref name="request"/> is not of <typeparamref name="TRequest"/> type.
    /// </exception>
    /// <exception cref="MissingRequestHandlerException">When handler for the provided <paramref name="request"/> was not found.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TResult Dispatch<TRequest, TResult>(this IRequestDispatcher dispatcher, IRequest<TRequest, TResult> request)
        where TRequest : class, IRequest<TRequest, TResult>
    {
        if ( ! dispatcher.TryDispatch( request, out var result ) )
            ThrowMissingRequestHandlerException( typeof( TRequest ) );

        return result;
    }

    /// <summary>
    /// Dispatches the provided <see cref="IRequest{TRequest,TResult}"/> instance.
    /// </summary>
    /// <param name="dispatcher">Source dispatcher.</param>
    /// <param name="request">Request to handle.</param>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResult">Request's result type.</typeparam>
    /// <returns>Result of the provided <paramref name="request"/>.</returns>
    /// <exception cref="MissingRequestHandlerException">When handler for the provided <paramref name="request"/> was not found.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TResult Dispatch<TRequest, TResult>(this IRequestDispatcher dispatcher, TRequest request)
        where TRequest : struct, IRequest<TRequest, TResult>
    {
        if ( ! dispatcher.TryDispatch<TRequest, TResult>( request, out var result ) )
            ThrowMissingRequestHandlerException( typeof( TRequest ) );

        return result;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowMissingRequestHandlerException(Type requestType)
    {
        throw new MissingRequestHandlerException( requestType );
    }
}
