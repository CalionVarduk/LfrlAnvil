using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping;

public readonly struct TypeMappingManyContext<TSource>
{
    internal TypeMappingManyContext(ITypeMapper typeMapper, IEnumerable<TSource> source)
    {
        TypeMapper = typeMapper;
        Source = source;
    }

    public ITypeMapper TypeMapper { get; }
    public IEnumerable<TSource> Source { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerable<TDestination> To<TDestination>()
    {
        return TypeMapper.MapMany<TSource, TDestination>( Source );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryTo<TDestination>([MaybeNullWhen( false )] out IEnumerable<TDestination> result)
    {
        return TypeMapper.TryMapMany( Source, out result );
    }
}
