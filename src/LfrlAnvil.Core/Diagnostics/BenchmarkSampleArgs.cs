namespace LfrlAnvil.Diagnostics;

public readonly record struct BenchmarkSampleArgs(BenchmarkSampleType Type, int Index, int Count, int Steps);
