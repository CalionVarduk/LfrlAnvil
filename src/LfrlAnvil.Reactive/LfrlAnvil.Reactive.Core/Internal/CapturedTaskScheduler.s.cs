using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Internal
{
    internal static class CapturedTaskScheduler
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static TaskScheduler GetCurrent()
        {
            return SynchronizationContext.Current is not null
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Current;
        }
    }
}
