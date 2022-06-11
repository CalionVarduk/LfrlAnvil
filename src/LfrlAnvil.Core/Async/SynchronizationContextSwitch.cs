using System;
using System.Threading;

namespace LfrlAnvil.Async
{
    public sealed class SynchronizationContextSwitch : IDisposable
    {
        public SynchronizationContextSwitch(SynchronizationContext? context)
        {
            PreviousContext = SynchronizationContext.Current;
            Context = context;
            SynchronizationContext.SetSynchronizationContext( Context );
        }

        public SynchronizationContext? PreviousContext { get; }
        public SynchronizationContext? Context { get; }

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext( PreviousContext );
        }
    }
}
