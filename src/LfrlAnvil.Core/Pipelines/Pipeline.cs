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
using System.Linq;

namespace LfrlAnvil.Pipelines;

/// <inheritdoc cref="IPipeline{TArg,TResult}" />
public class Pipeline<TArgs, TResult> : IPipeline<TArgs, TResult>
{
    private readonly IPipelineProcessor<TArgs, TResult>[] _processors;

    /// <summary>
    /// Creates a new <see cref="Pipeline{TArgs,TResult}"/> instance.
    /// </summary>
    /// <param name="processors">Sequence of attached pipeline processors.</param>
    /// <param name="defaultResult">Default result of this pipeline.</param>
    public Pipeline(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors, TResult defaultResult)
    {
        _processors = processors.ToArray();
        DefaultResult = defaultResult;
    }

    /// <inheritdoc />
    public TResult DefaultResult { get; }

    /// <inheritdoc />
    public virtual TResult Invoke(TArgs args)
    {
        var context = new PipelineContext<TArgs, TResult>( args, DefaultResult );
        ReinterpretCast.To<IPipelineProcessor<TArgs, TResult>>( this ).Process( context );
        return context.Result;
    }

    void IPipelineProcessor<TArgs, TResult>.Process(PipelineContext<TArgs, TResult> context)
    {
        foreach ( var processor in _processors )
        {
            processor.Process( context );
            if ( context.IsCompleted )
                break;
        }
    }
}
