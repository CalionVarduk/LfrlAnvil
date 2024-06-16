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
