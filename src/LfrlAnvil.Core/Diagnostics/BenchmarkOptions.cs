using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public readonly record struct BenchmarkOptions(BenchmarkSampleOptions Samples, double StandardScoreCutoff)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static double GetDefaultStandardScoreCutoff(int sampleCount)
    {
        var maxStandardScore = (sampleCount - 1) / Math.Sqrt( sampleCount );
        var result = Math.Min( maxStandardScore * 0.33, 1.5 );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BenchmarkOptions Create(BenchmarkSampleOptions samples)
    {
        return new BenchmarkOptions( samples, GetDefaultStandardScoreCutoff( samples.Count ) );
    }
}
