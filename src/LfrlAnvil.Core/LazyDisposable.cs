using System;
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// Represents a generic lazy container for an optional disposable object.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public sealed class LazyDisposable<T> : IDisposable
    where T : IDisposable
{
    private InterlockedBoolean _canAssign;
    private InterlockedBoolean _isDisposed;

    /// <summary>
    /// Creates a new <see cref="LazyDisposable{T}"/> instance without an <see cref="Inner"/> object.
    /// </summary>
    public LazyDisposable()
    {
        Inner = default;
        _canAssign = new InterlockedBoolean( true );
        _isDisposed = new InterlockedBoolean( false );
    }

    /// <summary>
    /// Optional underlying disposable object.
    /// </summary>
    public T? Inner { get; private set; }

    /// <summary>
    /// Specifies whether or not an underlying <see cref="Inner"/> object can be assigned to this instance.
    /// </summary>
    public bool CanAssign => _canAssign.Value;

    /// <summary>
    /// Specifies whether or not this instance has already been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed.Value;

    /// <inheritdoc />
    /// <remarks>Disposes the underlying <see cref="Inner"/> object if it exists.</remarks>
    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        if ( ! _canAssign.Value )
        {
            Assume.IsNotNull( Inner );
            Inner.Dispose();
        }
    }

    /// <summary>
    /// Assigns an object to the underlying <see cref="Inner"/> object.
    /// </summary>
    /// <param name="inner">Object to assign.</param>
    /// <exception cref="InvalidOperationException">When <see cref="Inner"/> object has already been assigned.</exception>
    /// <remarks>
    /// Will dispose the <see cref="Inner"/> object immediately upon assignment if this instance has been already disposed.
    /// </remarks>
    public void Assign(T inner)
    {
        if ( ! _canAssign.WriteFalse() )
            throw new InvalidOperationException( ExceptionResources.LazyDisposableCannotAssign );

        Inner = inner;
        if ( _isDisposed.Value )
            Inner.Dispose();
    }
}
