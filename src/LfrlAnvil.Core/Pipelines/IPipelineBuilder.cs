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

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Pipelines;

/// <summary>
/// Represents an <see cref="IPipeline{TArgs,TResult}"/> builder.
/// </summary>
/// <typeparam name="TArgs">Type of pipeline's input arguments.</typeparam>
/// <typeparam name="TResult">Type of pipeline's result.</typeparam>
public interface IPipelineBuilder<TArgs, TResult>
{
    /// <summary>
    /// Adds an <see cref="IPipelineProcessor{TArgs,TResult}"/> instance to this builder.
    /// </summary>
    /// <param name="processor">Processor to add.</param>
    /// <returns><b>this</b>.</returns>
    IPipelineBuilder<TArgs, TResult> Add(IPipelineProcessor<TArgs, TResult> processor);

    /// <summary>
    /// Adds a range of <see cref="IPipelineProcessor{TArgs,TResult}"/> instaces to this builder.
    /// </summary>
    /// <param name="processors">Range of processors to add.</param>
    /// <returns><b>this</b>.</returns>
    IPipelineBuilder<TArgs, TResult> Add(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors);

    /// <summary>
    /// Sets the default result for the pipeline.
    /// </summary>
    /// <param name="value">Default result.</param>
    /// <returns><b>this</b>.</returns>
    IPipelineBuilder<TArgs, TResult> SetDefaultResult(TResult value);

    /// <summary>
    /// Creates a new <see cref="IPipeline{TArgs,TResult}"/> instance.
    /// </summary>
    /// <returns>New <see cref="IPipeline{TArgs,TResult}"/> instance.</returns>
    [Pure]
    IPipeline<TArgs, TResult> Build();
}
