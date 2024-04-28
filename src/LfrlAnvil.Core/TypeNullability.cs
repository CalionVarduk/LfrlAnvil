using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

/// <summary>
/// Represents detailed information about <see cref="Type"/> nullability.
/// </summary>
public readonly struct TypeNullability : IEquatable<TypeNullability>
{
    private readonly Type? _actualType;
    private readonly Type? _underlyingType;

    private TypeNullability(bool isNullable, Type actualType, Type underlyingType)
    {
        IsNullable = isNullable;
        _actualType = actualType;
        _underlyingType = underlyingType;
    }

    /// <summary>
    /// Specifies whether or not the <see cref="ActualType"/> is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Specifies the actual <see cref="Type"/>.
    /// </summary>
    public Type ActualType => _actualType ?? typeof( object );

    /// <summary>
    /// Specifies the underlying <see cref="Type"/>. Will not be equal to <see cref="ActualType"/> only for nullable value types.
    /// </summary>
    public Type UnderlyingType => _underlyingType ?? typeof( object );

    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance.
    /// </summary>
    /// <param name="isNullable">Specifies whether or not the result should be nullable. Equal to <b>false</b> by default.</param>
    /// <typeparam name="T">Underlying type.</typeparam>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public static TypeNullability Create<T>(bool isNullable = false)
        where T : notnull
    {
        return typeof( T ).IsValueType
            ? CreateFromNonNullValueType( typeof( T ), isNullable )
            : CreateFromRefType( typeof( T ), isNullable );
    }

    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="isNullable">
    /// Specifies whether or not the result should be nullable. Equal to <b>false</b> by default. Will be ignored for nullable value types.
    /// </param>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public static TypeNullability Create(Type type, bool isNullable = false)
    {
        return type.IsValueType
            ? CreateFromValueType( type, isNullable )
            : CreateFromRefType( type, isNullable );
    }

    /// <summary>
    /// Returns a common <see cref="TypeNullability"/> instance for two instances.
    /// </summary>
    /// <param name="left">First type nullability.</param>
    /// <param name="right">Second type nullability.</param>
    /// <returns>
    /// null when <paramref name="left"/> is null or <paramref name="right"/> is null
    /// or <see cref="UnderlyingType"/> properties are not equal,
    /// otherwise <paramref name="left"/> when it is nullable, otherwise <paramref name="right"/>.
    /// </returns>
    [Pure]
    public static TypeNullability? GetCommonType(TypeNullability? left, TypeNullability? right)
    {
        return left?.GetCommonTypeWith( right );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="TypeNullability"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return IsNullable ? $"{nameof( Nullable )}<{UnderlyingType.GetDebugString()}>" : UnderlyingType.GetDebugString();
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( UnderlyingType, IsNullable );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TypeNullability t && Equals( t );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(TypeNullability other)
    {
        return UnderlyingType == other.UnderlyingType && IsNullable == other.IsNullable;
    }

    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance from this instance with <see cref="IsNullable"/> set to <b>true</b>.
    /// </summary>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public TypeNullability MakeNullable()
    {
        return IsNullable ? this : Create( UnderlyingType, isNullable: true );
    }

    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance from this instance with <see cref="IsNullable"/> set to <b>false</b>.
    /// </summary>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public TypeNullability MakeRequired()
    {
        return IsNullable ? Create( UnderlyingType, isNullable: false ) : this;
    }

    /// <summary>
    /// Returns a common <see cref="TypeNullability"/> instance for this instance and the provided <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other type nullability.</param>
    /// <returns>
    /// null when <paramref name="other"/> is null or <see cref="UnderlyingType"/> properties are not equal,
    /// otherwise <b>this</b> when it is nullable, otherwise <paramref name="other"/>.
    /// </returns>
    [Pure]
    public TypeNullability? GetCommonTypeWith(TypeNullability? other)
    {
        if ( other is null || UnderlyingType != other.Value.UnderlyingType )
            return null;

        return IsNullable ? this : other;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(TypeNullability a, TypeNullability b)
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
    public static bool operator !=(TypeNullability a, TypeNullability b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static TypeNullability CreateFromRefType(Type type, bool isNullable)
    {
        Assume.False( type.IsValueType );
        return new TypeNullability( isNullable, type, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static TypeNullability CreateFromValueType(Type type)
    {
        Assume.True( type.IsValueType );

        var underlyingNullableType = Nullable.GetUnderlyingType( type );
        return underlyingNullableType is not null
            ? new TypeNullability( isNullable: true, type, underlyingNullableType )
            : new TypeNullability( isNullable: false, type, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static TypeNullability CreateFromValueType(Type type, bool isNullable)
    {
        Assume.True( type.IsValueType );

        var underlyingNullableType = Nullable.GetUnderlyingType( type );
        return underlyingNullableType is not null
            ? new TypeNullability( isNullable: true, type, underlyingNullableType )
            : CreateFromNonNullValueType( type, isNullable );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static TypeNullability CreateFromNonNullValueType(Type type, bool isNullable)
    {
        Assume.True( type.IsValueType );
        var actualType = isNullable ? typeof( Nullable<> ).MakeGenericType( type ) : type;
        return new TypeNullability( isNullable, actualType, type );
    }
}
