using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a source that references an <see cref="ISqlObjectBuilder"/> instance.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlObjectBuilderReferenceSource<T> : IEquatable<SqlObjectBuilderReferenceSource<T>>
    where T : class, ISqlObjectBuilder
{
    internal SqlObjectBuilderReferenceSource(T @object, string? property)
    {
        Object = @object;
        Property = property;
    }

    /// <summary>
    /// Referencing object.
    /// </summary>
    public T Object { get; }

    /// <summary>
    /// Optional name of the referencing property of <see cref="Object"/>.
    /// </summary>
    public string? Property { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Property is null ? Object.ToString() ?? string.Empty : $"{Object} ({Property})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Object, Property );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlObjectBuilderReferenceSource<T> s && Equals( s );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlObjectBuilderReferenceSource<T> other)
    {
        return ReferenceEquals( Object, other.Object ) && Property == other.Property;
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderReferenceSource{T}"/> instance with changed <see cref="Property"/>.
    /// </summary>
    /// <param name="property">Optional name of the referencing property of <see cref="Object"/>.</param>
    /// <returns>New <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReferenceSource<T> WithProperty(string? property)
    {
        return new SqlObjectBuilderReferenceSource<T>( Object, property );
    }

    /// <summary>
    /// Converts this instance to another type that implements the <see cref="ISqlObjectBuilder"/> interface.
    /// </summary>
    /// <typeparam name="TDestination">SQL object builder type to convert to.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.</returns>
    /// <remarks>Be careful while using this method, because it does not actually validate the type's correctness.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderReferenceSource<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReferenceSource<TDestination>( ReinterpretCast.To<TDestination>( Object ), Property );
    }

    /// <summary>
    /// Converts <paramref name="source"/> to the base <see cref="ISqlObjectBuilder"/> type.
    /// </summary>
    /// <param name="source">Source to convert.</param>
    /// <returns>New <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlObjectBuilderReferenceSource<ISqlObjectBuilder>(SqlObjectBuilderReferenceSource<T> source)
    {
        return new SqlObjectBuilderReferenceSource<ISqlObjectBuilder>( source.Object, source.Property );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlObjectBuilderReferenceSource<T> a, SqlObjectBuilderReferenceSource<T> b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(SqlObjectBuilderReferenceSource<T> a, SqlObjectBuilderReferenceSource<T> b)
    {
        return ! a.Equals( b );
    }
}
