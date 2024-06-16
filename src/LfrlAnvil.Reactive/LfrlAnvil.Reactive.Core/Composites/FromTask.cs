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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Composites;

/// <summary>
/// Represents an event that is the result of a <see cref="Task{TResult}"/> completion.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public readonly struct FromTask<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="FromTask{TEvent}"/> instance.
    /// </summary>
    /// <param name="task">Source <see cref="Task{TResult}"/>.</param>
    public FromTask(Task<TEvent> task)
    {
        Result = task.Status == TaskStatus.RanToCompletion ? task.Result : default;
        Exception = task.Exception;
        Status = task.Status;
    }

    /// <summary>
    /// Status of the source task.
    /// </summary>
    public TaskStatus Status { get; }

    /// <summary>
    /// Result of the source task.
    /// </summary>
    public TEvent? Result { get; }

    /// <summary>
    /// Exception of the source task.
    /// </summary>
    public AggregateException? Exception { get; }

    /// <summary>
    /// Specifies whether or not the source task has been cancelled.
    /// </summary>
    public bool IsCanceled => Status == TaskStatus.Canceled;

    /// <summary>
    /// Specifies whether or not the source task has faulted.
    /// </summary>
    public bool IsFaulted => Status == TaskStatus.Faulted;

    /// <summary>
    /// Specifies whether or not the source task ran to completion.
    /// </summary>
    public bool IsCompletedSuccessfully => Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// Returns a string representation of this <see cref="FromTask{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var statusText = $"[{Status}]";

        if ( IsCanceled )
            return statusText;

        return IsFaulted ? $"{statusText}: {Exception}" : $"{statusText}: {Result}";
    }
}
