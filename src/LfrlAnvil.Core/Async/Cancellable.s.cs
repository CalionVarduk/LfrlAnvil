using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// Creates instances of <see cref="Cancellable{T}"/> type.
/// </summary>
public static class Cancellable
{
    /// <summary>
    /// Creates a <see cref="Cancellable{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to assign.</param>
    /// <param name="token"><see cref="CancellationToken"/> to assign.</param>
    /// <typeparam name="T">Value's type.</typeparam>
    /// <returns>A <see cref="Cancellable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Cancellable<T> Create<T>(T value, CancellationToken token)
    {
        return new Cancellable<T>( value, token );
    }
}
