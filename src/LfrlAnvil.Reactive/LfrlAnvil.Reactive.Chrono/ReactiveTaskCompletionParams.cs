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
using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents parameters of reactive task completion.
/// </summary>
/// <param name="Invocation">Invocation parameters.</param>
/// <param name="ElapsedTime">Amount of time taken for this invocation to end.</param>
/// <param name="Exception">Optional exception thrown by this invocation.</param>
/// <param name="CancellationReason">Optional cancellation reason that caused this task to end.</param>
public readonly record struct ReactiveTaskCompletionParams(
    ReactiveTaskInvocationParams Invocation,
    Duration ElapsedTime,
    Exception? Exception,
    TaskCancellationReason? CancellationReason
)
{
    /// <summary>
    /// Specifies whether or not the invocation ended with an error.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Exception ) )]
    public bool IsFailed => Exception is not null;

    /// <summary>
    /// Specifies whether or not the invocation ended successfully.
    /// </summary>
    public bool IsSuccessful => ! IsFailed && CancellationReason is null;
}
