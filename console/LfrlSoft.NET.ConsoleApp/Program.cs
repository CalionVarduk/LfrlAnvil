using System;
using System.Linq;

namespace LfrlSoft.NET.ConsoleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var x = Array.Empty<int>();
            var y = x.Aggregate( 0, (a, b) => a + b );
            Console.WriteLine( "Hello World!" );
        }
    }
}
