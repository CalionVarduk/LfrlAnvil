using BenchmarkDotNet.Running;
using System;

namespace LfrlSoft.NET.Common.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StructEqualityBenchmark>();
            Console.ReadKey( true );
        }
    }
}
