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
