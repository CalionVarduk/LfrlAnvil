using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Requests.Exceptions;

namespace LfrlAnvil.Requests;

public sealed class RequestDispatcher : IRequestDispatcher
{
    private readonly IRequestHandlerFactory _handlerFactory;

    public RequestDispatcher(IRequestHandlerFactory handlerFactory)
    {
        _handlerFactory = handlerFactory;
    }

    public TResult Dispatch<TRequest, TResult>(IRequest<TRequest, TResult> request)
        where TRequest : class, IRequest<TRequest, TResult>
    {
        return DispatchInternal<TRequest, TResult>( CastToRequest( request ) );
    }

    public TResult Dispatch<TRequest, TResult>(TRequest request)
        where TRequest : struct, IRequest<TRequest, TResult>
    {
        return DispatchInternal<TRequest, TResult>( request );
    }

    public bool TryDispatch<TRequest, TResult>(IRequest<TRequest, TResult> request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : class, IRequest<TRequest, TResult>
    {
        return TryDispatchInternal( CastToRequest( request ), out result );
    }

    public bool TryDispatch<TRequest, TResult>(TRequest request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : struct, IRequest<TRequest, TResult>
    {
        return TryDispatchInternal( request, out result );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private TResult DispatchInternal<TRequest, TResult>(TRequest request)
        where TRequest : IRequest<TRequest, TResult>
    {
        if ( ! TryDispatchInternal<TRequest, TResult>( request, out var result ) )
            throw MissingRequestHandlerException<TRequest>();

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryDispatchInternal<TRequest, TResult>(TRequest request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : IRequest<TRequest, TResult>
    {
        var handler = _handlerFactory.TryCreate<TRequest, TResult>();
        if ( handler is null )
        {
            result = default;
            return false;
        }

        result = handler.Handle( request );
        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static TRequest CastToRequest<TRequest, TResult>(IRequest<TRequest, TResult> request)
        where TRequest : class, IRequest<TRequest, TResult>
    {
        return request as TRequest ?? throw InvalidRequestTypeException<TRequest>( request.GetType() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static MissingRequestHandlerException MissingRequestHandlerException<TRequest>()
    {
        return new MissingRequestHandlerException( typeof( TRequest ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static InvalidRequestTypeException InvalidRequestTypeException<TRequest>(Type requestType)
    {
        return new InvalidRequestTypeException( requestType, typeof( TRequest ) );
    }
}
