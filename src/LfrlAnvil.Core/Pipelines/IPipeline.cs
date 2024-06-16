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

namespace LfrlAnvil.Pipelines;

/// <summary>
/// Represents a pipeline object that accepts input arguments and returns a result.
/// </summary>
/// <typeparam name="TArgs">Type of input arguments.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IPipeline<TArgs, TResult> : IPipelineProcessor<TArgs, TResult>
{
    /// <summary>
    /// Specifies default result of this pipeline.
    /// </summary>
    TResult DefaultResult { get; }

    /// <summary>
    /// Invokes this pipeline.
    /// </summary>
    /// <param name="args">Input arguments.</param>
    /// <returns>Calculated result.</returns>
    /// <remarks>
    /// Sequentially invokes all <see cref="IPipelineProcessor{TArgs,TResult}"/> instances attached to this pipeline,
    /// unless the <see cref="PipelineContext{TArgs,TResult}"/> gets marked as completed, which stops the invocation.
    /// </remarks>
    TResult Invoke(TArgs args);
}
