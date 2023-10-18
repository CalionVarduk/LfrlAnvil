using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public struct BenchmarkStatisticsCollection
{
    public readonly record struct StatisticInfo(long StartTimestamp, long StartAllocatedBytes, bool CollectGarbage = false);

    internal readonly record struct MemoryEntry(long Min, long Max, double Mean, double VarianceBase);

    internal readonly MemoryEntry[] AllocatedBytes;
    internal readonly long[,] ElapsedTimeTicks;

    internal BenchmarkStatisticsCollection(int samples, int stepsPerSample)
    {
        Assume.IsGreaterThan( samples, 0, nameof( samples ) );
        Assume.IsGreaterThan( stepsPerSample, 0, nameof( stepsPerSample ) );

        AllocatedBytes = new MemoryEntry[stepsPerSample];
        ElapsedTimeTicks = new long[samples, stepsPerSample];
        SampleIndex = 0;
        NextStepIndex = stepsPerSample;
    }

    internal int SampleIndex { get; private set; }
    internal int NextStepIndex { get; private set; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Add(StatisticInfo info)
    {
        var endTimestamp = BenchmarkHelpers.GetTimestamp();
        if ( info.CollectGarbage )
            BenchmarkHelpers.CollectGarbage();

        var endAllocatedBytes = BenchmarkHelpers.GetAllocatedBytes();
        Ensure.IsInRange( NextStepIndex, 0, AllocatedBytes.Length - 1, nameof( NextStepIndex ) );

        AllocatedBytes[NextStepIndex] = GetNextMemoryEntry( AllocatedBytes[NextStepIndex], endAllocatedBytes - info.StartAllocatedBytes );
        ElapsedTimeTicks[SampleIndex, NextStepIndex] = StopwatchTimestamp.GetTicks( info.StartTimestamp, endTimestamp );
        ++NextStepIndex;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Reset()
    {
        Array.Fill( AllocatedBytes, new MemoryEntry( long.MaxValue, long.MinValue, 0.0, 0.0 ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetSample(int sampleIndex)
    {
        Assume.IsInRange( sampleIndex, 0, ElapsedTimeTicks.GetLength( 0 ) - 1, nameof( sampleIndex ) );
        SampleIndex = sampleIndex;
        NextStepIndex = 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private MemoryEntry GetNextMemoryEntry(MemoryEntry previous, long value)
    {
        var min = Math.Min( previous.Min, value );
        var max = Math.Max( previous.Max, value );
        var mean = BenchmarkHelpers.GetNextMean( previous.Mean, value, SampleIndex );
        var varianceBase = BenchmarkHelpers.GetNextVarianceBase( previous.VarianceBase, value, previous.Mean, mean );
        return new MemoryEntry( min, max, mean, varianceBase );
    }
}
