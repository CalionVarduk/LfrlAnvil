using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Process
{
    public sealed class ProcessRunner : IProcessRunner
    {
        private readonly IProcessHandlerFactory _handlerFactory;

        public ProcessRunner(IProcessHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory;
        }

        public TResult Run<TArgs, TResult>(TArgs args)
            where TArgs : IProcessArgs<TResult>
        {
            if ( ! TryRun<TArgs, TResult>( args, out var result ) )
                throw MissingProcessHandlerException<TArgs>();

            return result;
        }

        public bool TryRun<TArgs, TResult>(TArgs args, [MaybeNullWhen( false )] out TResult result)
            where TArgs : IProcessArgs<TResult>
        {
            var handler = _handlerFactory.TryCreate<TArgs, TResult>();
            if ( handler is null )
            {
                result = default;
                return false;
            }

            result = handler.Handle( args );
            return true;
        }

        public ValueTask<TResult> RunAsync<TArgs, TResult>(TArgs args, CancellationToken cancellationToken)
            where TArgs : IProcessArgs<TResult>
        {
            var result = TryRunAsync<TArgs, TResult>( args, cancellationToken );
            if ( result is null )
                throw MissingProcessHandlerException<TArgs>();

            return result.Value;
        }

        public ValueTask<TResult>? TryRunAsync<TArgs, TResult>(TArgs args, CancellationToken cancellationToken)
            where TArgs : IProcessArgs<TResult>
        {
            var handler = _handlerFactory.TryCreateAsync<TArgs, TResult>();
            return handler?.Handle( args, cancellationToken );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static MissingProcessHandlerException MissingProcessHandlerException<TArgs>()
        {
            return new MissingProcessHandlerException( typeof( TArgs ) );
        }
    }
}
