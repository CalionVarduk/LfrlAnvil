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
