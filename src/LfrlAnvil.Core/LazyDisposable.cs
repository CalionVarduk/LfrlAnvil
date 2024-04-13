using System;
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

public sealed class LazyDisposable<T> : IDisposable
    where T : IDisposable
{
    private InterlockedBoolean _canAssign;
    private InterlockedBoolean _isDisposed;

    public LazyDisposable()
    {
        Inner = default;
        _canAssign = new InterlockedBoolean( true );
        _isDisposed = new InterlockedBoolean( false );
    }

    public T? Inner { get; private set; }
    public bool CanAssign => _canAssign.Value;
    public bool IsDisposed => _isDisposed.Value;

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

    public void Assign(T inner)
    {
        if ( ! _canAssign.WriteFalse() )
            throw new LazyDisposableAssignmentException();

        Inner = inner;
        if ( _isDisposed.Value )
            Inner.Dispose();
    }
}
