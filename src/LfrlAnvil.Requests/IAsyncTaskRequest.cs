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

using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Requests;

/// <summary>
/// Represents an asynchronous single generic request. This is the <see cref="Task{T}"/> version.
/// </summary>
/// <typeparam name="TRequest">Request type. Use the "Curiously Recurring Template Pattern" (CRTP) approach.</typeparam>
/// <typeparam name="TResult">Request's result type.</typeparam>
public interface IAsyncTaskRequest<TRequest, TResult> : IRequest<TRequest, Task<TResult>>
    where TRequest : IRequest<TRequest, Task<TResult>>
{
    /// <summary>
    /// Request's cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }
}
