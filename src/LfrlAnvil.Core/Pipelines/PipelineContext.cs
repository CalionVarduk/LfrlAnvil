using System.Runtime.CompilerServices;

namespace LfrlAnvil.Pipelines;

/// <summary>
/// Represents a context for pipeline processing.
/// </summary>
/// <typeparam name="TArgs">Type of pipeline's input arguments.</typeparam>
/// <typeparam name="TResult">Type of pipeline's result.</typeparam>
public sealed class PipelineContext<TArgs, TResult>
{
    internal PipelineContext(TArgs args, TResult result)
    {
        Args = args;
        Result = result;
        IsCompleted = false;
    }

    /// <summary>
    /// Input arguments provided to the pipeline.
    /// </summary>
    public TArgs Args { get; }

    /// <summary>
    /// Current result stored by this context.
    /// </summary>
    public TResult Result { get; private set; }

    /// <summary>
    /// Specifies whether or not this context has been marked as complete.
    /// Pipeline invocation will stop when it detects a completed context.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Marks this context as complete. Pipeline invocation will stop when it detects a completed context.
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Complete()
    {
        IsCompleted = true;
    }

    /// <summary>
    /// Sets the result of this context.
    /// </summary>
    /// <param name="result">New result.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetResult(TResult result)
    {
        Result = result;
    }
}
