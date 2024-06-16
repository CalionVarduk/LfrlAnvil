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

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a strategy for capturing a task scheduler.
/// </summary>
public enum TaskSchedulerCaptureStrategy : byte
{
    /// <summary>
    /// Specifies that the task scheduler should not be captured at all.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies that the current task scheduler should be captured.
    /// </summary>
    Current = 1,

    /// <summary>
    /// Specifies that the task scheduler should not be captured but should be returned on demand.
    /// </summary>
    Lazy = 2
}
