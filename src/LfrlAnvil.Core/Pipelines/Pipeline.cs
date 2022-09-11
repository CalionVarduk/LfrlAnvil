using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Pipelines;

public class Pipeline<TArgs, TResult> : IPipeline<TArgs, TResult>
{
    private readonly IPipelineProcessor<TArgs, TResult>[] _processors;

    public Pipeline(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors, TResult defaultResult)
    {
        _processors = processors.ToArray();
        DefaultResult = defaultResult;
    }

    public TResult DefaultResult { get; }

    public virtual TResult Invoke(TArgs args)
    {
        var context = new PipelineContext<TArgs, TResult>( args, DefaultResult );
        ReinterpretCast.To<IPipelineProcessor<TArgs, TResult>>( this ).Process( context );
        return context.Result;
    }

    void IPipelineProcessor<TArgs, TResult>.Process(PipelineContext<TArgs, TResult> context)
    {
        foreach ( var processor in _processors )
        {
            processor.Process( context );
            if ( context.IsCompleted )
                break;
        }
    }
}
