using BenchmarkDotNet.Running;
using CommandLine;
using LfrlSoft.NET.Common.Benchmarks.CL;
using System;
using System.Linq;

namespace LfrlSoft.NET.Common.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineBenchmarkOptions>( args )
                .WithParsed( o =>
                 {
                     var benchmarkLocator = new BenchmarkLocator();
                     var types = benchmarkLocator.LocateTypes( o );

                     if ( !types.Any() )
                         Console.WriteLine( "No benchmark option has been provided." );

                     foreach ( var t in types )
                         BenchmarkRunner.Run( t );
                 } )
                .WithNotParsed( e =>
                 {
                     var error = string.Join( Environment.NewLine, e );
                     Console.WriteLine( error );
                 } );

            Console.ReadKey( true );
        }
    }
}
