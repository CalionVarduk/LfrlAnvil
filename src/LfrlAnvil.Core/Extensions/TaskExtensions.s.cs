// Copyright 2025 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Task"/> and <see cref="ValueTask"/> extension methods.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Returns the provided <paramref name="task"/>, unless it is null, in which case returns <see cref="Task.CompletedTask"/> instead.
    /// </summary>
    /// <param name="task">Source task.</param>
    /// <returns><paramref name="task"/> if it is not null, otherwise <see cref="Task.CompletedTask"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Task CompletedIfNull(this Task? task)
    {
        return task ?? Task.CompletedTask;
    }

    /// <summary>
    /// Safely awaits for the provided <paramref name="task"/> to complete.
    /// </summary>
    /// <param name="task">Task to await.</param>
    /// <returns><see cref="ValueTask{TResult}"/> instance with an underlying <see cref="Result"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<Result> AsSafe(this Task task)
    {
        try
        {
            await task.ConfigureAwait( false );
            return Result.Valid;
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }

    /// <summary>
    /// Safely awaits for the provided <paramref name="task"/> to complete.
    /// </summary>
    /// <param name="task">Task to await.</param>
    /// <returns><see cref="ValueTask{TResult}"/> instance with an underlying <see cref="Result"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<Result> AsSafe(this ValueTask task)
    {
        try
        {
            await task.ConfigureAwait( false );
            return Result.Valid;
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }

    /// <summary>
    /// Safely awaits for the provided <paramref name="task"/> to complete.
    /// </summary>
    /// <param name="task">Task to await.</param>
    /// <typeparam name="TResult">Task result type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> instance with an underlying <see cref="Result{T}"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<Result<TResult>> AsSafe<TResult>(this Task<TResult> task)
    {
        try
        {
            var result = await task.ConfigureAwait( false );
            return result;
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }

    /// <summary>
    /// Safely awaits for the provided <paramref name="task"/> to complete.
    /// </summary>
    /// <param name="task">Task to await.</param>
    /// <typeparam name="TResult">Task result type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> instance with an underlying <see cref="Result{T}"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<Result<TResult>> AsSafe<TResult>(this ValueTask<TResult> task)
    {
        try
        {
            var result = await task.ConfigureAwait( false );
            return result;
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }
}
