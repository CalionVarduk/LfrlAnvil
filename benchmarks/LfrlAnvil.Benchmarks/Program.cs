using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Running;
using CommandLine;
using LfrlAnvil.Benchmarks.CL;

namespace LfrlAnvil.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineBenchmarkOptions>( args )
                .WithParsed( RunBenchmarks )
                .WithNotParsed( ShowErrors );

            Console.ReadKey( true );
        }

        private static void RunBenchmarks(CommandLineBenchmarkOptions options)
        {
            var benchmarkLocator = new BenchmarkLocator();
            var types = benchmarkLocator.LocateTypes( options );

            if ( ! types.Any() )
                Console.WriteLine( "No benchmark option has been provided." );

            foreach ( var t in types )
                BenchmarkRunner.Run( t );
        }

        private static void ShowErrors(IEnumerable<Error> errors)
        {
            var error = string.Join( Environment.NewLine, errors );
            Console.WriteLine( error );
        }
    }
}
