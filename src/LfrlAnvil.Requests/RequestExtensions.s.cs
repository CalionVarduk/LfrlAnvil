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
