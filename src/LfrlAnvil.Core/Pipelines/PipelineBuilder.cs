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
