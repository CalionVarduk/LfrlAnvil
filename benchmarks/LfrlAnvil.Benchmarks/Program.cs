using System;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using CommandLine;
using LfrlAnvil.Benchmarks.CL;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Benchmarks
{
    internal class Program
    {
        private static void ModifyBenchmarksToRun(CommandLineBenchmarkOptions options)
        {
            options.StructEquality = true;
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineBenchmarkOptions>( args )
                .WithParsed( RunBenchmarks )
                .WithNotParsed( ShowErrors );

            Console.ReadKey( true );
        }

        private static void RunBenchmarks(CommandLineBenchmarkOptions options)
        {
            ModifyBenchmarksToRun( options );

            var types = BenchmarkLocator.LocateTypes( options ).Materialize();

            if ( types.Count == 0 )
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
