using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

internal static class BenchmarkHelpers
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void CollectGarbage()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static long GetAllocatedBytes()
    {
        return GC.GetAllocatedBytesForCurrentThread();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static double GetNextMean(double previous, double sample, int sampleIndex)
    {
        return previous + (sample - previous) / (sampleIndex + 1);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static double GetNextVarianceBase(double previous, double sample, double previousMean, double currentMean)
    {
        return previous + (sample - previousMean) * (sample - currentMean);
    }
}
