using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Pipelines;

public class PipelineBuilder<TArgs, TResult> : IPipelineBuilder<TArgs, TResult>
{
    private readonly List<IPipelineProcessor<TArgs, TResult>> _processors;
    private TResult _defaultResult;

    public PipelineBuilder(TResult defaultResult)
    {
        _processors = new List<IPipelineProcessor<TArgs, TResult>>();
        _defaultResult = defaultResult;
    }

    public IPipelineBuilder<TArgs, TResult> Add(IPipelineProcessor<TArgs, TResult> processor)
    {
        _processors.Add( processor );
        return this;
    }

    public IPipelineBuilder<TArgs, TResult> Add(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors)
    {
        _processors.AddRange( processors );
        return this;
    }

    public IPipelineBuilder<TArgs, TResult> SetDefaultResult(TResult value)
    {
        _defaultResult = value;
        return this;
    }

    [Pure]
    public IPipeline<TArgs, TResult> Build()
    {
        return new Pipeline<TArgs, TResult>( _processors, _defaultResult );
    }
}
