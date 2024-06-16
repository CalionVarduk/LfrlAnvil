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

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a reason for reactive task cancellation.
/// </summary>
public enum TaskCancellationReason : byte
{
    /// <summary>
    /// Specifies that a task has been cancelled due to <see cref="CancellationToken"/>.
    /// </summary>
    CancellationRequested = 0,

    /// <summary>
    /// Specifies that a task has been cancelled due to reached maximum task invocation queue size limit.
    /// </summary>
    MaxQueueSizeLimit = 1,

    /// <summary>
    /// Specifies that a task has been cancelled due to its definition being disposed.
    /// </summary>
    TaskDisposed = 2
}
