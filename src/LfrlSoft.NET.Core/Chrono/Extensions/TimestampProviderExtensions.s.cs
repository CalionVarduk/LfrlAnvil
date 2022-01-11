using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class TimestampProviderExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsInPast(this ITimestampProvider provider, Timestamp timestamp)
        {
            return timestamp < provider.GetNow();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNow(this ITimestampProvider provider, Timestamp timestamp)
        {
            return timestamp == provider.GetNow();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsInFuture(this ITimestampProvider provider, Timestamp timestamp)
        {
            return timestamp > provider.GetNow();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration GetDifference(this ITimestampProvider provider, Timestamp timestamp)
        {
            return timestamp - provider.GetNow();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration GetDifference(this ITimestampProvider provider, ITimestampProvider other)
        {
            return provider.GetDifference( other.GetNow() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ITimestampProvider Freeze(this ITimestampProvider provider)
        {
            return new FrozenTimestampProvider( provider.GetNow() );
        }
    }
}
