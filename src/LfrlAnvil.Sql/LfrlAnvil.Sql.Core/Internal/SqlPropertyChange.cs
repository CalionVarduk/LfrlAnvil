using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlPropertyChange<T>
{
    private readonly bool _isActive;

    internal SqlPropertyChange(bool isActive, T newValue, object? state)
    {
        _isActive = isActive;
        NewValue = newValue;
        State = state;
    }

    public T NewValue { get; }
    public object? State { get; }
    public bool IsCancelled => ! _isActive;

    [Pure]
    public override string ToString()
    {
        var typeText = typeof( T ).GetDebugString();
        if ( IsCancelled )
            return $"Cancel<{typeText}>()";

        return Generic<T>.IsNull( NewValue ) ? $"SetNull<{typeText}>()" : $"Set<{typeText}>({NewValue})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlPropertyChange<T>(T newValue)
    {
        return new SqlPropertyChange<T>( isActive: true, newValue, state: null );
    }
}
