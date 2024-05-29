using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Pipelines;

/// <inheritdoc cref="IPipeline{TArg,TResult}" />
public class Pipeline<TArgs, TResult> : IPipeline<TArgs, TResult>
{
    private readonly IPipelineProcessor<TArgs, TResult>[] _processors;

    /// <summary>
    /// Creates a new <see cref="Pipeline{TArgs,TResult}"/> instance.
    /// </summary>
    /// <param name="processors">Sequence of attached pipeline processors.</param>
    /// <param name="defaultResult">Default result of this pipeline.</param>
    public Pipeline(IEnumerable<IPipelineProcessor<TArgs, TResult>> processors, TResult defaultResult)
    {
        _processors = processors.ToArray();
        DefaultResult = defaultResult;
    }

    /// <inheritdoc />
    public TResult DefaultResult { get; }

    /// <inheritdoc />
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
