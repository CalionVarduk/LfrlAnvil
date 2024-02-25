using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

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

    public bool IsNullable { get; }
    public Type ActualType => _actualType ?? typeof( object );
    public Type UnderlyingType => _underlyingType ?? typeof( object );

    [Pure]
    public static TypeNullability Create<T>(bool isNullable = false)
        where T : notnull
    {
        return typeof( T ).IsValueType
            ? CreateFromNonNullValueType( typeof( T ), isNullable )
            : CreateFromRefType( typeof( T ), isNullable );
    }

    [Pure]
    public static TypeNullability Create(Type type, bool isNullable = false)
    {
        return type.IsValueType
            ? CreateFromValueType( type, isNullable )
            : CreateFromRefType( type, isNullable );
    }

    [Pure]
    public static TypeNullability? GetCommonType(TypeNullability? left, TypeNullability? right)
    {
        return left?.GetCommonTypeWith( right );
    }

    [Pure]
    public override string ToString()
    {
        return IsNullable ? $"{nameof( Nullable )}<{UnderlyingType.GetDebugString()}>" : UnderlyingType.GetDebugString();
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( UnderlyingType, IsNullable );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TypeNullability t && Equals( t );
    }

    [Pure]
    public bool Equals(TypeNullability other)
    {
        return UnderlyingType == other.UnderlyingType && IsNullable == other.IsNullable;
    }

    [Pure]
    public TypeNullability MakeNullable()
    {
        return IsNullable ? this : Create( UnderlyingType, isNullable: true );
    }

    [Pure]
    public TypeNullability MakeRequired()
    {
        return IsNullable ? Create( UnderlyingType, isNullable: false ) : this;
    }

    [Pure]
    public TypeNullability? GetCommonTypeWith(TypeNullability? other)
    {
        if ( other is null || UnderlyingType != other.Value.UnderlyingType )
            return null;

        return IsNullable ? this : other;
    }

    [Pure]
    public static bool operator ==(TypeNullability a, TypeNullability b)
    {
        return a.Equals( b );
    }

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
