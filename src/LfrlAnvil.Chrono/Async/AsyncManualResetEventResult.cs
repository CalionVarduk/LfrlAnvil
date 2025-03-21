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

namespace LfrlAnvil.Chrono.Async;

/// <summary>
/// Represents possible values returned by <see cref="AsyncManualResetEvent"/> tasks.
/// </summary>
public enum AsyncManualResetEventResult : byte
{
    /// <summary>
    /// Specifies that the manual reset event or its owner has been disposed.
    /// </summary>
    Disposed = 0,

    /// <summary>
    /// Specifies that the manual reset event task has completed due to timeout.
    /// </summary>
    TimedOut = 1,

    /// <summary>
    /// Specifies that the manual reset event task has been cancelled due to the event being in the signaled state.
    /// </summary>
    Signaled = 2,

    /// <summary>
    /// Specifies that the manual reset event task creation has been cancelled due to the event already being awaited.
    /// </summary>
    AlreadyAwaited = 3
}
