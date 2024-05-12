using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object that specifies how <see cref="SqlObjectBuilder"/> property change attempt should be handled.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct SqlPropertyChange<T>
{
    private readonly bool _isActive;

    internal SqlPropertyChange(bool isActive, T newValue, object? state)
    {
        _isActive = isActive;
        NewValue = newValue;
        State = state;
    }

    /// <summary>
    /// New value to set.
    /// </summary>
    public T NewValue { get; }

    /// <summary>
    /// Custom state.
    /// </summary>
    public object? State { get; }

    /// <summary>
    /// Specifies whether or not the change should be ignored.
    /// </summary>
    public bool IsCancelled => ! _isActive;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlPropertyChange{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var typeText = typeof( T ).GetDebugString();
        if ( IsCancelled )
            return $"Cancel<{typeText}>()";

        return Generic<T>.IsNull( NewValue ) ? $"SetNull<{typeText}>()" : $"Set<{typeText}>({NewValue})";
    }

    /// <summary>
    /// Creates a new <see cref="SqlPropertyChange{T}"/> instance.
    /// </summary>
    /// <param name="newValue">New value to set.</param>
    /// <returns>New <see cref="SqlPropertyChange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlPropertyChange<T>(T newValue)
    {
        return new SqlPropertyChange<T>( isActive: true, newValue, state: null );
    }
}
