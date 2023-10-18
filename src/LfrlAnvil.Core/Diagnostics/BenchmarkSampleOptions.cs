using System.Diagnostics.Contracts;

namespace LfrlAnvil.Diagnostics;

public readonly record struct BenchmarkSampleOptions(int Count, int StepsPerSample, bool CollectGarbage = true)
{
    [Pure]
    public static implicit operator BenchmarkOptions(BenchmarkSampleOptions o)
    {
        return BenchmarkOptions.Create( o );
    }
}
