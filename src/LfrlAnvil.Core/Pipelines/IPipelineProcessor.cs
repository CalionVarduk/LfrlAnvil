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
/// Represents an object capable of processing a <see cref="PipelineContext{TArgs,TResult}"/> instance.
/// </summary>
/// <typeparam name="TArgs">Type of context's input arguments.</typeparam>
/// <typeparam name="TResult">Type of context's result.</typeparam>
public interface IPipelineProcessor<TArgs, TResult>
{
    /// <summary>
    /// Processes the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">Context to process.</param>
    void Process(PipelineContext<TArgs, TResult> context);
}
