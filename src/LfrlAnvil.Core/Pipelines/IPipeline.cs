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
