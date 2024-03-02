using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlScalarQueryResult : IEquatable<SqlScalarQueryResult>
{
    public static readonly SqlScalarQueryResult Empty = new SqlScalarQueryResult();

    public SqlScalarQueryResult(object? value)
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
        return obj is SqlScalarQueryResult r && Equals( r );
    }

    [Pure]
    public bool Equals(SqlScalarQueryResult other)
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
    public static bool operator ==(SqlScalarQueryResult left, SqlScalarQueryResult right)
    {
        return left.Equals( right );
    }

    [Pure]
    public static bool operator !=(SqlScalarQueryResult left, SqlScalarQueryResult right)
    {
        return ! left.Equals( right );
    }
}

public readonly struct SqlScalarQueryResult<T> : IEquatable<SqlScalarQueryResult<T>>
{
    public static readonly SqlScalarQueryResult<T> Empty = new SqlScalarQueryResult<T>();

    public SqlScalarQueryResult(T? value)
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
        return obj is SqlScalarQueryResult<T> r && Equals( r );
    }

    [Pure]
    public bool Equals(SqlScalarQueryResult<T> other)
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
    public static bool operator ==(SqlScalarQueryResult<T> left, SqlScalarQueryResult<T> right)
    {
        return left.Equals( right );
    }

    [Pure]
    public static bool operator !=(SqlScalarQueryResult<T> left, SqlScalarQueryResult<T> right)
    {
        return ! left.Equals( right );
    }
}
