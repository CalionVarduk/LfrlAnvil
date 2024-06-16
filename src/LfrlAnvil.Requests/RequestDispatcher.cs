// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Requests.Exceptions;

namespace LfrlAnvil.Requests;

/// <inheritdoc cref="IRequestDispatcher" />
public sealed class RequestDispatcher : IRequestDispatcher
{
    private readonly IRequestHandlerFactory _handlerFactory;

    /// <summary>
    /// Creates a new <see cref="RequestDispatcher"/> instance.
    /// </summary>
    /// <param name="handlerFactory">Request handler factory instance.</param>
    public RequestDispatcher(IRequestHandlerFactory handlerFactory)
    {
        _handlerFactory = handlerFactory;
    }

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
    public bool TryDispatch<TRequest, TResult>(IRequest<TRequest, TResult> request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : class, IRequest<TRequest, TResult>
    {
        return TryDispatchInternal( CastToRequest( request ), out result );
    }

    /// <inheritdoc />
    public bool TryDispatch<TRequest, TResult>(TRequest request, [MaybeNullWhen( false )] out TResult result)
        where TRequest : struct, IRequest<TRequest, TResult>
    {
        return TryDispatchInternal( request, out result );
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
        if ( request is TRequest result )
            return result;

        ThrowInvalidRequestTypeException( request.GetType(), typeof( TRequest ) );
        return default!;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowInvalidRequestTypeException(Type requestType, Type expectedType)
    {
        throw new InvalidRequestTypeException( requestType, expectedType );
    }
}
