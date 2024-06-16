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

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents the state of <see cref="ReactiveScheduler{TKey}"/>.
/// </summary>
public enum ReactiveSchedulerState
{
    /// <summary>
    /// Specifies that the scheduler has not been started yet.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Specifies that the scheduler is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Specifies that the scheduler is currently in the process of being disposed.
    /// </summary>
    Stopping = 2,

    /// <summary>
    /// Specifies that the scheduler has been disposed.
    /// </summary>
    Disposed = 3
}
