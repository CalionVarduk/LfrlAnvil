namespace LfrlAnvil.Pipelines;

public interface IPipeline<TArgs, TResult> : IPipelineProcessor<TArgs, TResult>
{
    TResult DefaultResult { get; }
    TResult Invoke(TArgs args);
}
