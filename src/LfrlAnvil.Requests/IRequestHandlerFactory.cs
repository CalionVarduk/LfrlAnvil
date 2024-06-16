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
