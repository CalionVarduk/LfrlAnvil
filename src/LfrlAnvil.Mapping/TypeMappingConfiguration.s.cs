using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping
{
    public static class TypeMappingConfiguration
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TypeMappingConfiguration<TSource, TDestination> Create<TSource, TDestination>(
            Func<TSource, ITypeMapper, TDestination> mapping)
        {
            return new TypeMappingConfiguration<TSource, TDestination>( mapping );
        }
    }
}
