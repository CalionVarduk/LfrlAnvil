using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public abstract class Benchmark<TState>
{
    protected Benchmark(BenchmarkOptions options, BenchmarkSampleOptions? warmupOptions = null)
    {
        Ensure.IsGreaterThanOrEqualTo( options.Samples.Count, 0 );
        Ensure.IsGreaterThanOrEqualTo( options.Samples.StepsPerSample, 0 );

        Options = options;
        if ( warmupOptions is not null )
        {
            Ensure.IsGreaterThanOrEqualTo( warmupOptions.Value.Count, 0 );
            Ensure.IsGreaterThanOrEqualTo( warmupOptions.Value.StepsPerSample, 0 );
            WarmupOptions = warmupOptions.Value;
        }
        else
            WarmupOptions = options.Samples;
    }

    public BenchmarkOptions Options { get; }
    public BenchmarkSampleOptions WarmupOptions { get; }

    [Pure]
    public BenchmarkStepInfo[] Run()
    {
        if ( Options.Samples.Count == 0 || Options.Samples.StepsPerSample == 0 )
            return Array.Empty<BenchmarkStepInfo>();

        var result = RunCore();
        BenchmarkHelpers.CollectGarbage();
        return result;
    }

    [Pure]
    public BenchmarkSampleOptions GetSampleOptions(BenchmarkSampleType type)
    {
        return type == BenchmarkSampleType.Actual ? Options.Samples : WarmupOptions;
    }

    [Pure]
    protected abstract TState CreateState();

    [MethodImpl( MethodImplOptions.NoInlining )]
    protected abstract void OnSample(TState state, BenchmarkSampleArgs args, ref BenchmarkStatisticsCollection statistics);

    protected virtual void OnBeforeAll(BenchmarkSampleType? sampleType) { }
    protected virtual void OnAfterAll(BenchmarkSampleType? sampleType, TimeSpan elapsedTime) { }
    protected virtual void OnBeforeSample(TState state, BenchmarkSampleArgs args) { }
    protected virtual void OnAfterSample(TState state, BenchmarkSampleArgs args, TimeSpan elapsedTime) { }
    protected virtual void OnBeforeResultExtraction() { }
    protected virtual void OnAfterResultExtraction(TimeSpan elapsedTime) { }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static long GetTimestamp()
    {
        return BenchmarkHelpers.GetTimestamp();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static BenchmarkStatisticsCollection.StatisticInfo GetStartInfo(bool collectGarbage = false)
    {
        var allocatedBytes = BenchmarkHelpers.GetAllocatedBytes();
        return new BenchmarkStatisticsCollection.StatisticInfo( GetTimestamp(), allocatedBytes, collectGarbage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static TimeSpan GetElapsedTime(long startTimestamp)
    {
        return StopwatchTimestamp.GetTimeSpan( startTimestamp, GetTimestamp() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.NoInlining )]
    private BenchmarkStepInfo[] RunCore()
    {
        var start = GetTimestamp();
        OnBeforeAll( null );

        var state = CreateState();
        BenchmarkHelpers.CollectGarbage();

        var statistics = new BenchmarkStatisticsCollection(
            Math.Max( Options.Samples.Count, WarmupOptions.Count ),
            Math.Max( Options.Samples.StepsPerSample, WarmupOptions.StepsPerSample ) );

        if ( WarmupOptions.Count > 0 && WarmupOptions.StepsPerSample > 0 )
            Run( state, BenchmarkSampleType.Warmup, WarmupOptions, ref statistics );

        Run( state, BenchmarkSampleType.Actual, Options.Samples, ref statistics );

        var resultStart = GetTimestamp();
        OnBeforeResultExtraction();
        var result = GetResult( statistics );
        OnAfterResultExtraction( GetElapsedTime( resultStart ) );
        OnAfterAll( null, GetElapsedTime( start ) );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Run(
        TState state,
        BenchmarkSampleType sampleType,
        BenchmarkSampleOptions options,
        ref BenchmarkStatisticsCollection statistics)
    {
        var start = GetTimestamp();
        OnBeforeAll( sampleType );
        statistics.Reset();

        for ( var s = 0; s < options.Count; ++s )
        {
            var sampleArgs = new BenchmarkSampleArgs( sampleType, s, options.Count, options.StepsPerSample );
            var sampleStart = GetTimestamp();
            OnBeforeSample( state, sampleArgs );
            statistics.ResetSample( s );

            if ( options.CollectGarbage )
                BenchmarkHelpers.CollectGarbage();

            OnSample( state, sampleArgs, ref statistics );
            OnAfterSample( state, sampleArgs, GetElapsedTime( sampleStart ) );
        }

        BenchmarkHelpers.CollectGarbage();
        OnAfterAll( sampleType, GetElapsedTime( start ) );
    }

    [Pure]
    private BenchmarkStepInfo[] GetResult(BenchmarkStatisticsCollection statistics)
    {
        var sampleOptions = Options.Samples;
        var steps = new BenchmarkStepInfo[sampleOptions.StepsPerSample];
        var elapsedTimeSamples = new long[sampleOptions.Count];
        var samplesSqrt = Math.Sqrt( sampleOptions.Count );

        for ( var i = 0; i < sampleOptions.StepsPerSample; ++i )
        {
            var allocatedBytesEntry = statistics.AllocatedBytes[i];
            var variance = allocatedBytesEntry.VarianceBase / sampleOptions.Count;
            var standardDeviation = Math.Sqrt( variance );
            var allocatedBytes = new AggregateStatistic<MemorySize>(
                Min: MemorySize.FromBytes( allocatedBytesEntry.Min ),
                Max: MemorySize.FromBytes( allocatedBytesEntry.Max ),
                Mean: MemorySize.FromBytes( allocatedBytesEntry.Mean ),
                Variance: MemorySize.FromBytes( variance ),
                StandardDeviation: MemorySize.FromBytes( standardDeviation ),
                StandardError: MemorySize.FromBytes( standardDeviation / samplesSqrt ) );

            var minElapsedTime = long.MaxValue;
            var maxElapsedTime = long.MinValue;
            var elapsedTimeMean = 0.0;
            var elapsedTimeVarianceBase = 0.0;

            for ( var s = 0; s < sampleOptions.Count; ++s )
            {
                var value = statistics.ElapsedTimeTicks[s, i];
                var previousElapsedTimeMean = elapsedTimeMean;
                elapsedTimeMean = BenchmarkHelpers.GetNextMean( elapsedTimeMean, value, s );
                elapsedTimeVarianceBase = BenchmarkHelpers.GetNextVarianceBase(
                    elapsedTimeVarianceBase,
                    value,
                    previousElapsedTimeMean,
                    elapsedTimeMean );

                minElapsedTime = Math.Min( minElapsedTime, value );
                maxElapsedTime = Math.Max( maxElapsedTime, value );
                elapsedTimeSamples[s] = value;
            }

            variance = elapsedTimeVarianceBase / sampleOptions.Count;
            standardDeviation = Math.Sqrt( variance );
            var elapsedTimeWithOutliers = new AggregateStatistic<TimeSpan>(
                Min: TimeSpan.FromTicks( minElapsedTime ),
                Max: TimeSpan.FromTicks( maxElapsedTime ),
                Mean: TimeSpan.FromTicks( ( long )Math.Ceiling( elapsedTimeMean ) ),
                Variance: TimeSpan.FromTicks( ( long )Math.Ceiling( variance ) ),
                StandardDeviation: TimeSpan.FromTicks( ( long )Math.Ceiling( standardDeviation ) ),
                StandardError: TimeSpan.FromTicks( ( long )Math.Ceiling( standardDeviation / samplesSqrt ) ) );

            minElapsedTime = long.MaxValue;
            maxElapsedTime = long.MinValue;
            elapsedTimeVarianceBase = 0.0;
            var actualElapsedTimeMean = 0.0;
            var actualElapsedTimeSampleCount = 0;

            for ( var s = 0; s < sampleOptions.Count; ++s )
            {
                var value = elapsedTimeSamples[s];
                var standardScore = (value - elapsedTimeMean) / standardDeviation;
                if ( Math.Abs( standardScore ) >= Options.StandardScoreCutoff )
                    continue;

                minElapsedTime = Math.Min( minElapsedTime, value );
                maxElapsedTime = Math.Max( maxElapsedTime, value );

                var previousActualElapsedTimeMean = actualElapsedTimeMean;
                actualElapsedTimeMean = BenchmarkHelpers.GetNextMean( actualElapsedTimeMean, value, actualElapsedTimeSampleCount++ );
                elapsedTimeVarianceBase = BenchmarkHelpers.GetNextVarianceBase(
                    elapsedTimeVarianceBase,
                    value,
                    previousActualElapsedTimeMean,
                    actualElapsedTimeMean );
            }

            variance = actualElapsedTimeSampleCount > 0 ? elapsedTimeVarianceBase / actualElapsedTimeSampleCount : 0.0;
            standardDeviation = Math.Sqrt( variance );
            var elapsedTime = new AggregateStatistic<TimeSpan>(
                Min: TimeSpan.FromTicks( minElapsedTime ),
                Max: TimeSpan.FromTicks( maxElapsedTime ),
                Mean: TimeSpan.FromTicks( ( long )Math.Ceiling( actualElapsedTimeMean ) ),
                Variance: TimeSpan.FromTicks( ( long )Math.Ceiling( variance ) ),
                StandardDeviation: TimeSpan.FromTicks( ( long )Math.Ceiling( standardDeviation ) ),
                StandardError: TimeSpan.FromTicks(
                    actualElapsedTimeSampleCount > 0
                        ? ( long )Math.Ceiling( standardDeviation / Math.Sqrt( actualElapsedTimeSampleCount ) )
                        : 0 ) );

            steps[i] = new BenchmarkStepInfo(
                allocatedBytes,
                elapsedTimeWithOutliers,
                elapsedTime,
                sampleOptions.Count - actualElapsedTimeSampleCount );
        }

        return steps;
    }
}
