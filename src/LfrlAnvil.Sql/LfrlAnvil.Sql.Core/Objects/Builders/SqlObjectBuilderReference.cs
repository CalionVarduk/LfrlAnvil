using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlObjectBuilderReference<T>
    where T : class, ISqlObjectBuilder
{
    internal SqlObjectBuilderReference(SqlObjectBuilderReferenceSource<T> source, T target)
    {
        Source = source;
        Target = target;
    }

    public SqlObjectBuilderReferenceSource<T> Source { get; }
    public T Target { get; }

    [Pure]
    public override string ToString()
    {
        return $"{Source} => {Target}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReference<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReference<TDestination>(
            Source.UnsafeReinterpretAs<TDestination>(),
            ReinterpretCast.To<TDestination>( Target ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlObjectBuilderReference<ISqlObjectBuilder>(SqlObjectBuilderReference<T> source)
    {
        return new SqlObjectBuilderReference<ISqlObjectBuilder>( source.Source, source.Target );
    }
}
