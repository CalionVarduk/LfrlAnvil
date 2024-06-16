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

namespace LfrlAnvil.Pipelines;

/// <summary>
/// Represents a context for pipeline processing.
/// </summary>
/// <typeparam name="TArgs">Type of pipeline's input arguments.</typeparam>
/// <typeparam name="TResult">Type of pipeline's result.</typeparam>
public sealed class PipelineContext<TArgs, TResult>
{
    internal PipelineContext(TArgs args, TResult result)
    {
        Args = args;
        Result = result;
        IsCompleted = false;
    }

    /// <summary>
    /// Input arguments provided to the pipeline.
    /// </summary>
    public TArgs Args { get; }

    /// <summary>
    /// Current result stored by this context.
    /// </summary>
    public TResult Result { get; private set; }

    /// <summary>
    /// Specifies whether or not this context has been marked as complete.
    /// Pipeline invocation will stop when it detects a completed context.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Marks this context as complete. Pipeline invocation will stop when it detects a completed context.
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Complete()
    {
        IsCompleted = true;
    }

    /// <summary>
    /// Sets the result of this context.
    /// </summary>
    /// <param name="result">New result.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetResult(TResult result)
    {
        Result = result;
    }
}
