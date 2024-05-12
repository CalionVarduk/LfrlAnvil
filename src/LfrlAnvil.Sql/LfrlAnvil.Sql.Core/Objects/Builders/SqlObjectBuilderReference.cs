using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a reference between two SQL object builders.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlObjectBuilderReference<T>
    where T : class, ISqlObjectBuilder
{
    internal SqlObjectBuilderReference(SqlObjectBuilderReferenceSource<T> source, T target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Underlying reference source.
    /// </summary>
    public SqlObjectBuilderReferenceSource<T> Source { get; }

    /// <summary>
    /// Target object builder.
    /// </summary>
    public T Target { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObjectBuilderReference{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Source} => {Target}";
    }

    /// <summary>
    /// Converts this instance to another type that implements the <see cref="ISqlObjectBuilder"/> interface.
    /// </summary>
    /// <typeparam name="TDestination">SQL object builder type to convert to.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderReference{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReference<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReference<TDestination>(
            Source.UnsafeReinterpretAs<TDestination>(),
            ReinterpretCast.To<TDestination>( Target ) );
    }

    /// <summary>
    /// Converts <paramref name="source"/> to the base <see cref="ISqlObjectBuilder"/> type.
    /// </summary>
    /// <param name="source">Source to convert.</param>
    /// <returns>New <see cref="SqlObjectBuilderReference{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlObjectBuilderReference<ISqlObjectBuilder>(SqlObjectBuilderReference<T> source)
    {
        return new SqlObjectBuilderReference<ISqlObjectBuilder>( source.Source, source.Target );
    }
}
