using System;
using LfrlSoft.NET.Core.Extensions;

namespace LfrlSoft.NET.ConsoleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var a = "abc".ToMaybe();
            var b = "def".ToMaybe();

            var r = a.Match(
                some: av => b.IfSomeOrDefault( bv => av == bv ),
                none: () => ! b.HasValue );

            Console.WriteLine( "Hello World!" );
        }
    }
}
