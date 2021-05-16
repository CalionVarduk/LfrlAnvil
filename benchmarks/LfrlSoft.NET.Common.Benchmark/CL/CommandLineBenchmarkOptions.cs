using CommandLine;

namespace LfrlSoft.NET.Common.Benchmarks.CL
{
    public class CommandLineBenchmarkOptions
    {
        [CommandLineBenchmark( typeof( StructEqualityBenchmark ) )]
        [Option( "struct-equality", Default = false, Required = false, HelpText = "Run struct type equality benchmark." )]
        public bool StructEquality { get; set; }

        [CommandLineBenchmark( typeof( RefEqualityBenchmark ) )]
        [Option( "ref-equality", Default = false, Required = false, HelpText = "Run class type equality benchmark." )]
        public bool RefEquality { get; set; }

        [CommandLineBenchmark( typeof( DefaultComparisonBenchmark ) )]
        [Option( "default-comparison", Default = false, Required = false, HelpText = "Run comparison with default value benchmark." )]
        public bool DefaultComparison { get; set; }
    }
}
