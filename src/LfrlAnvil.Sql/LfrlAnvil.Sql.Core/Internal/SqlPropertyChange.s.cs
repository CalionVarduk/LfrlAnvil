using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Creates instances of <see cref="SqlPropertyChange{T}"/> type.
/// </summary>
public static class SqlPropertyChange
{
    /// <summary>
    /// Creates a new <see cref="SqlPropertyChange{T}"/> instance.
    /// </summary>
    /// <param name="newValue">New value to set.</param>
    /// <param name="state">Optional custom state. Equal to null by default.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlPropertyChange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPropertyChange<T> Create<T>(T newValue, object? state = null)
    {
        return new SqlPropertyChange<T>( isActive: true, newValue, state );
    }

    /// <summary>
    /// Creates a new <see cref="SqlPropertyChange{T}"/> instance marked as cancelled.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlPropertyChange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPropertyChange<T> Cancel<T>()
    {
        return new SqlPropertyChange<T>( isActive: false, default!, state: null );
    }
}
