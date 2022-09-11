using System.Runtime.CompilerServices;

namespace LfrlAnvil.Pipelines;

public sealed class PipelineContext<TArgs, TResult>
{
    internal PipelineContext(TArgs args, TResult result)
    {
        Args = args;
        Result = result;
        IsCompleted = false;
    }

    public TArgs Args { get; }
    public TResult Result { get; private set; }
    public bool IsCompleted { get; private set; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Complete()
    {
        IsCompleted = true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetResult(TResult result)
    {
        Result = result;
    }
}
