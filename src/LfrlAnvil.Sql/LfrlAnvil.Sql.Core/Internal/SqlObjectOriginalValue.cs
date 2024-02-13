using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlObjectOriginalValue<T>
{
    private SqlObjectOriginalValue(T? value, bool exists)
    {
        Value = value;
        Exists = exists;
    }

    public T? Value { get; }
    public bool Exists { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectOriginalValue<T> Create(T value)
    {
        return new SqlObjectOriginalValue<T>( value, exists: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectOriginalValue<T> CreateEmpty()
    {
        return new SqlObjectOriginalValue<T>( value: default, exists: false );
    }

    [Pure]
    public override string ToString()
    {
        return Exists ? $"Value<{typeof( T ).GetDebugString()}>({Value})" : $"Empty<{typeof( T ).GetDebugString()}>()";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T GetValueOrDefault(T @default)
    {
        return Exists ? Value! : @default;
    }
}
