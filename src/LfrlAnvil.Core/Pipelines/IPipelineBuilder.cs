using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Pipelines;

public interface IPipelineBuilder<TArgs, TResult>
{
    IPipelineBuilder<TArgs, TResult> Add(IPipelineProcessor<TArgs, TResult> processor);
    IPipelineBuilder<TArgs, TResult> Add(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors);
    IPipelineBuilder<TArgs, TResult> SetDefaultResult(TResult value);

    [Pure]
    IPipeline<TArgs, TResult> Build();
}
