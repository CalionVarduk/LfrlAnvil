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
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight object capable of extracting <see cref="TaskScheduler"/> instances from the current <see cref="SynchronizationContext"/>
/// and capturing them.
/// </summary>
public readonly struct TaskSchedulerCapture
{
    private readonly TaskScheduler? _scheduler;
    private readonly bool _lazyCapture;

    /// <summary>
    /// Creates a new <see cref="TaskSchedulerCapture"/> instance with an explicit <see cref="TaskScheduler"/>.
    /// </summary>
    /// <param name="scheduler">An explicit <see cref="TaskScheduler"/> instance to capture.</param>
    public TaskSchedulerCapture(TaskScheduler? scheduler)
    {
        _scheduler = scheduler;
        _lazyCapture = false;
    }

    /// <summary>
    /// Creates a new <see cref="TaskSchedulerCapture"/> instance based on the provided <paramref name="strategy"/>.
    /// </summary>
    /// <param name="strategy">An option that defines the <see cref="TaskSchedulerCapture"/> instance's behavior.</param>
    /// <remarks>See <see cref="TaskSchedulerCaptureStrategy"/> for available strategies.</remarks>
    public TaskSchedulerCapture(TaskSchedulerCaptureStrategy strategy)
    {
        switch ( strategy )
        {
            case TaskSchedulerCaptureStrategy.Current:
                _scheduler = GetCurrentScheduler();
                _lazyCapture = false;
                break;

            case TaskSchedulerCaptureStrategy.Lazy:
                _scheduler = null;
                _lazyCapture = true;
                break;

            default:
                _scheduler = null;
                _lazyCapture = false;
                break;
        }
    }

    /// <summary>
    /// Returns the current <see cref="TaskScheduler"/> instance.
    /// </summary>
    /// <returns>Current <see cref="TaskScheduler"/> instance.</returns>
    /// <remarks>
    /// When <see cref="SynchronizationContext.Current"/> synchronization context is not null,
    /// then this method returns the result of <see cref="TaskScheduler.FromCurrentSynchronizationContext()"/> invocation,
    /// otherwise returns <see cref="TaskScheduler.Current"/> task scheduler.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskScheduler GetCurrentScheduler()
    {
        return SynchronizationContext.Current is not null
            ? TaskScheduler.FromCurrentSynchronizationContext()
            : TaskScheduler.Current;
    }

    /// <summary>
    /// Returns <see cref="TaskScheduler"/> instance associated with the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// <see cref="SynchronizationContext"/> instance from which to get the <see cref="TaskScheduler"/> instance.
    /// </param>
    /// <returns><see cref="TaskScheduler"/> instance associated with the provided <paramref name="context"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskScheduler FromSynchronizationContext(SynchronizationContext context)
    {
        using var contextSwitch = new SynchronizationContextSwitch( context );
        return TaskScheduler.FromCurrentSynchronizationContext();
    }

    /// <summary>
    /// Returns the captured <see cref="TaskScheduler"/> instance or the current <see cref="TaskScheduler"/>
    /// if this capture has been created with the <see cref="TaskSchedulerCaptureStrategy.Lazy"/> option.
    /// </summary>
    /// <returns><see cref="TaskScheduler"/> associated with this capture.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TaskScheduler? TryGetScheduler()
    {
        return _lazyCapture ? GetCurrentScheduler() : _scheduler;
    }
}
