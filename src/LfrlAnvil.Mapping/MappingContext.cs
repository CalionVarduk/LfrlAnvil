using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping
{
    public readonly struct MappingContext<TSource>
    {
        internal MappingContext(ITypeMapper typeMapper, TSource source)
        {
            TypeMapper = typeMapper;
            Source = source;
        }

        public ITypeMapper TypeMapper { get; }
        public TSource Source { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TDestination To<TDestination>()
        {
            return TypeMapper.Map<TSource, TDestination>( Source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryTo<TDestination>([MaybeNullWhen( false )] out TDestination result)
        {
            return TypeMapper.TryMap( Source, out result );
        }
    }
}
