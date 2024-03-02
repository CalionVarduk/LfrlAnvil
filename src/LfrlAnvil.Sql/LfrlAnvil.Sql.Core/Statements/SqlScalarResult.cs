using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlScalarResult : IEquatable<SqlScalarResult>
{
    public static readonly SqlScalarResult Empty = new SqlScalarResult();

    public SqlScalarResult(object? value)
    {
        HasValue = true;
        Value = value;
    }

    public bool HasValue { get; }
    public object? Value { get; }

    [Pure]
    public override string ToString()
    {
        return HasValue ? $"{nameof( Value )}({Value})" : $"{nameof( Empty )}()";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( HasValue, Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlScalarResult r && Equals( r );
    }

    [Pure]
    public bool Equals(SqlScalarResult other)
    {
        if ( ! HasValue )
            return ! other.HasValue;

        return other.HasValue && Equals( Value, other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValue()
    {
        return HasValue ? Value : throw new InvalidOperationException( ExceptionResources.ScalarResultDoesNotHaveValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValueOrDefault(object? @default)
    {
        return HasValue ? Value : @default;
    }

    [Pure]
    public static bool operator ==(SqlScalarResult left, SqlScalarResult right)
    {
        return left.Equals( right );
    }

    [Pure]
    public static bool operator !=(SqlScalarResult left, SqlScalarResult right)
    {
        return ! left.Equals( right );
    }
}

public readonly struct SqlScalarResult<T> : IEquatable<SqlScalarResult<T>>
{
    public static readonly SqlScalarResult<T> Empty = new SqlScalarResult<T>();

    public SqlScalarResult(T? value)
    {
        HasValue = true;
        Value = value;
    }

    public bool HasValue { get; }
    public T? Value { get; }

    [Pure]
    public override string ToString()
    {
        var typeText = typeof( T ).GetDebugString();
        return HasValue ? $"{nameof( Value )}<{typeText}>({Value})" : $"{nameof( Empty )}<{typeText}>()";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( HasValue, Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlScalarResult<T> r && Equals( r );
    }

    [Pure]
    public bool Equals(SqlScalarResult<T> other)
    {
        if ( ! HasValue )
            return ! other.HasValue;

        return other.HasValue && Generic<T>.AreEqual( Value, other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? GetValue()
    {
        return HasValue ? Value : throw new InvalidOperationException( ExceptionResources.ScalarResultDoesNotHaveValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? GetValueOrDefault(T? @default)
    {
        return HasValue ? Value : @default;
    }

    [Pure]
    public static bool operator ==(SqlScalarResult<T> left, SqlScalarResult<T> right)
    {
        return left.Equals( right );
    }

    [Pure]
    public static bool operator !=(SqlScalarResult<T> left, SqlScalarResult<T> right)
    {
        return ! left.Equals( right );
    }
}
