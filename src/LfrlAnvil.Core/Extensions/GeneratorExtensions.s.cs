using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Extensions
{
    public static class GeneratorExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable ToEnumerable(this IGenerator source)
        {
            while ( source.TryGenerate( out var result ) )
                yield return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> ToEnumerable<T>(this IGenerator<T> source)
        {
            while ( source.TryGenerate( out var result ) )
                yield return result;
        }
    }
}
