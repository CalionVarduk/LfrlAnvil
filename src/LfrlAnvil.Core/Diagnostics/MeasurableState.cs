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

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// Represents the state of a <see cref="Measurable"/> instance.
/// </summary>
public enum MeasurableState : byte
{
    /// <summary>
    /// Specifies that a measurable has not yet been invoked.
    /// </summary>
    Ready = 0,

    /// <summary>
    /// Specifies that a measurable has been invoked and is in its preparation stage.
    /// </summary>
    Preparing = 1,

    /// <summary>
    /// Specifies that a measurable has been invoked and is in its main state.
    /// </summary>
    Running = 2,

    /// <summary>
    /// Specifies that a measurable has been invoked and is in its teardown stage.
    /// </summary>
    TearingDown = 3,

    /// <summary>
    /// Specifies that a measurable instance invocation has finished.
    /// </summary>
    Done = 4
}
