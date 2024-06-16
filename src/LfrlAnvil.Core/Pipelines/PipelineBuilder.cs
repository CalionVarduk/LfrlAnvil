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

/// <inheritdoc cref="IPipelineBuilder{TArg,TResult}" />
public class PipelineBuilder<TArgs, TResult> : IPipelineBuilder<TArgs, TResult>
{
    private readonly List<IPipelineProcessor<TArgs, TResult>> _processors;
    private TResult _defaultResult;

    /// <summary>
    /// Creates a new <see cref="PipelineBuilder{TArgs,TResult}"/> instance.
    /// </summary>
    /// <param name="defaultResult">Default result of the pipeline.</param>
    public PipelineBuilder(TResult defaultResult)
    {
        _processors = new List<IPipelineProcessor<TArgs, TResult>>();
        _defaultResult = defaultResult;
    }

    /// <inheritdoc />
    public IPipelineBuilder<TArgs, TResult> Add(IPipelineProcessor<TArgs, TResult> processor)
    {
        _processors.Add( processor );
        return this;
    }

    /// <inheritdoc />
    public IPipelineBuilder<TArgs, TResult> Add(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors)
    {
        _processors.AddRange( processors );
        return this;
    }

    /// <inheritdoc />
    public IPipelineBuilder<TArgs, TResult> SetDefaultResult(TResult value)
    {
        _defaultResult = value;
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IPipeline<TArgs, TResult> Build()
    {
        return new Pipeline<TArgs, TResult>( _processors, _defaultResult );
    }
}
