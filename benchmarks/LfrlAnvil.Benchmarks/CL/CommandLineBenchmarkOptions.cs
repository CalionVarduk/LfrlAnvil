using CommandLine;

namespace LfrlAnvil.Benchmarks.CL
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

        [CommandLineBenchmark( typeof( SafeBitCastBenchmark ) )]
        [Option( "bit-cast", Default = false, Required = false, HelpText = "Run bit cast benchmark." )]
        public bool BitCast { get; set; }

        [CommandLineBenchmark( typeof( BackingFieldBenchmark ) )]
        [Option( "backing-field", Default = false, Required = false, HelpText = "Run backing field access benchmark." )]
        public bool BackingField { get; set; }
    }
}
