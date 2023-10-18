namespace LfrlAnvil.Diagnostics;

public readonly record struct BenchmarkSampleArgs(BenchmarkSampleType Type, int SampleIndex, int Samples, int Steps);
