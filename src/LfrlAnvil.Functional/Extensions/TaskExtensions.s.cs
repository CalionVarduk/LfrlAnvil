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

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains various task extension methods.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Creates a new <see cref="Task{TResult}"/> that returns a <see cref="Nil"/> instance.
    /// </summary>
    /// <param name="task">Source task.</param>
    /// <returns>New <see cref="Task{TResult}"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async Task<Nil> ToNil(this Task task)
    {
        await task;
        return Nil.Instance;
    }

    /// <summary>
    /// Creates a new <see cref="ValueTask{TResult}"/> that returns a <see cref="Nil"/> instance.
    /// </summary>
    /// <param name="task">Source task.</param>
    /// <returns>New <see cref="ValueTask{TResult}"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<Nil> ToNil(this ValueTask task)
    {
        await task;
        return Nil.Instance;
    }

    /// <summary>
    /// Creates a new <see cref="Task"/> without a result.
    /// </summary>
    /// <param name="task">Source task.</param>
    /// <typeparam name="T">Ignored result type.</typeparam>
    /// <returns>New <see cref="Task"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async Task IgnoreResult<T>(this Task<T> task)
    {
        await task;
    }

    /// <summary>
    /// Creates a new <see cref="ValueTask"/> without a result.
    /// </summary>
    /// <param name="task">Source task.</param>
    /// <typeparam name="T">Ignored result type.</typeparam>
    /// <returns>New <see cref="ValueTask"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask IgnoreResult<T>(this ValueTask<T> task)
    {
        await task;
    }
}
