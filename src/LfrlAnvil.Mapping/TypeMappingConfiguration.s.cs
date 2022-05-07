using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping
{
    public partial class TypeMappingConfiguration
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SingleTypeMappingConfiguration<TSource, TDestination> Create<TSource, TDestination>(
            Func<TSource, ITypeMapper, TDestination> mapping)
        {
            return new SingleTypeMappingConfiguration<TSource, TDestination>( mapping );
        }
    }
}
