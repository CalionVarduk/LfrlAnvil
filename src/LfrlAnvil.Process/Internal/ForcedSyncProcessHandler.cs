using System.Threading;

namespace LfrlAnvil.Process.Internal
{
    internal sealed class ForcedSyncProcessHandler<TArgs, TResult> : IProcessHandler<TArgs, TResult>
        where TArgs : IProcessArgs<TResult>
    {
        private readonly IAsyncProcessHandler<TArgs, TResult> _base;

        internal ForcedSyncProcessHandler(IAsyncProcessHandler<TArgs, TResult> @base)
        {
            _base = @base;
        }

        public TResult Handle(TArgs args)
        {
            return _base.Handle( args, CancellationToken.None ).AsTask().Result;
        }
    }
}
