using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Process.Internal;

namespace LfrlAnvil.Process.Extensions
{
    public static class ProcessHandlerExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IProcessHandler<TArgs, TResult> ToSynchronous<TArgs, TResult>(this IAsyncProcessHandler<TArgs, TResult> source)
            where TArgs : IProcessArgs<TResult>
        {
            return new ForcedSyncProcessHandler<TArgs, TResult>( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IAsyncProcessHandler<TArgs, TResult> ToAsynchronous<TArgs, TResult>(this IProcessHandler<TArgs, TResult> source)
            where TArgs : IProcessArgs<TResult>
        {
            return new ForcedAsyncProcessHandler<TArgs, TResult>( source );
        }
    }
}
