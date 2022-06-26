using System;

namespace LfrlAnvil.Benchmarks.CL;

[AttributeUsage( AttributeTargets.Property )]
public class CommandLineBenchmarkAttribute : Attribute
{
    public CommandLineBenchmarkAttribute(Type benchmarkType)
    {
        BenchmarkType = benchmarkType;
    }

    public Type BenchmarkType { get; }
}