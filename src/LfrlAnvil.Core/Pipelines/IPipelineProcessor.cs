namespace LfrlAnvil.Pipelines;

public interface IPipelineProcessor<TArgs, TResult>
{
    void Process(PipelineContext<TArgs, TResult> context);
}
