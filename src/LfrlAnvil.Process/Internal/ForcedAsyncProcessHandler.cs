using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Process.Internal
{
    internal sealed class ForcedAsyncProcessHandler<TArgs, TResult> : IAsyncProcessHandler<TArgs, TResult>
        where TArgs : IProcessArgs<TResult>
    {
        private readonly IProcessHandler<TArgs, TResult> _base;

        internal ForcedAsyncProcessHandler(IProcessHandler<TArgs, TResult> @base)
        {
            _base = @base;
        }

        public ValueTask<TResult> Handle(TArgs args, CancellationToken _)
        {
            return new ValueTask<TResult>( _base.Handle( args ) );
        }
    }
}
