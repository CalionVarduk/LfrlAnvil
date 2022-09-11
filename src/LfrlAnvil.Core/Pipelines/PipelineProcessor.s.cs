using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Pipelines;

public static class PipelineProcessor
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IPipelineProcessor<TArgs, TResult> Create<TArgs, TResult>(Action<PipelineContext<TArgs, TResult>> action)
    {
        return new Lambda<TArgs, TResult>( action );
    }

    private sealed class Lambda<TArgs, TResult> : IPipelineProcessor<TArgs, TResult>
    {
        private readonly Action<PipelineContext<TArgs, TResult>> _action;

        internal Lambda(Action<PipelineContext<TArgs, TResult>> action)
        {
            _action = action;
        }

        public void Process(PipelineContext<TArgs, TResult> context)
        {
            _action( context );
        }
    }
}
