using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Pipelines;

/// <summary>
/// Creates instances of <see cref="IPipelineProcessor{TArgs,TResult}"/> type.
/// </summary>
public static class PipelineProcessor
{
    /// <summary>
    /// Creates a new <see cref="IPipelineProcessor{TArgs,TResult}"/> instance from a delegate.
    /// </summary>
    /// <param name="action">Processor's action.</param>
    /// <typeparam name="TArgs">Type of pipeline's input arguments.</typeparam>
    /// <typeparam name="TResult">Type of pipeline's result.</typeparam>
    /// <returns>New <see cref="IPipelineProcessor{TArgs,TResult}"/> instance.</returns>
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
