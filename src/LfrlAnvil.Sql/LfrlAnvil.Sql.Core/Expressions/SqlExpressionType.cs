using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public readonly struct SqlExpressionType : IEquatable<SqlExpressionType>
{
    private readonly Type? _baseType;
    private readonly Type? _fullType;

    private SqlExpressionType(Type baseType, Type fullType, bool isNullable)
    {
        _baseType = baseType;
        _fullType = fullType;
        IsNullable = isNullable;
    }

    public bool IsNullable { get; }
    public Type BaseType => _baseType ?? typeof( object );
    public Type FullType => _fullType ?? typeof( object );

    [Pure]
    public static SqlExpressionType Create<T>(bool isNullable = false)
        where T : notnull
    {
        return typeof( T ).IsValueType
            ? CreateFromNonNullValueType( typeof( T ), isNullable )
            : CreateFromRefType( typeof( T ), isNullable );
    }

    [Pure]
    public static SqlExpressionType Create(Type type, bool isNullable = false)
    {
        return type.IsValueType
            ? CreateFromValueType( type, isNullable )
            : CreateFromRefType( type, isNullable );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExpressionType? GetCommonType(SqlExpressionType? left, SqlExpressionType? right)
    {
        return left?.GetCommonTypeWith( right );
    }

    [Pure]
    public override string ToString()
    {
        return IsNullable ? $"{nameof( Nullable )}<{BaseType.GetDebugString()}>" : BaseType.GetDebugString();
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( BaseType, IsNullable );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlExpressionType t && Equals( t );
    }

    [Pure]
    public bool Equals(SqlExpressionType other)
    {
        return BaseType == other.BaseType && IsNullable == other.IsNullable;
    }

    [Pure]
    public SqlExpressionType MakeNullable()
    {
        return IsNullable ? this : Create( BaseType, isNullable: true );
    }

    [Pure]
    public SqlExpressionType MakeRequired()
    {
        return IsNullable ? Create( BaseType, isNullable: false ) : this;
    }

    [Pure]
    public SqlExpressionType? GetCommonTypeWith(SqlExpressionType? other)
    {
        if ( other is null )
            return null;

        if ( BaseType == other.Value.BaseType )
            return IsNullable ? this : other;

        if ( BaseType == typeof( DBNull ) )
            return other.Value.IsNullable ? other : Create( other.Value.BaseType, isNullable: true );

        if ( other.Value.BaseType == typeof( DBNull ) )
            return IsNullable ? this : Create( BaseType, isNullable: true );

        return null;
    }

    [Pure]
    public static bool operator ==(SqlExpressionType a, SqlExpressionType b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(SqlExpressionType a, SqlExpressionType b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlExpressionType CreateFromRefType(Type type, bool isNullable)
    {
        Assume.Equals( type.IsValueType, false, nameof( type.IsValueType ) );
        return new SqlExpressionType( type, type, isNullable && type != typeof( DBNull ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlExpressionType CreateFromValueType(Type type, bool isNullable)
    {
        Assume.Equals( type.IsValueType, true, nameof( type.IsValueType ) );

        var underlyingNullableType = Nullable.GetUnderlyingType( type );
        return underlyingNullableType is not null
            ? new SqlExpressionType( underlyingNullableType, type, isNullable: true )
            : CreateFromNonNullValueType( type, isNullable );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlExpressionType CreateFromNonNullValueType(Type type, bool isNullable)
    {
        Assume.Equals( type.IsValueType, true, nameof( type.IsValueType ) );
        var fullType = isNullable ? typeof( Nullable<> ).MakeGenericType( type ) : type;
        return new SqlExpressionType( type, fullType, isNullable );
    }
}
