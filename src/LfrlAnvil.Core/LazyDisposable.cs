using System;
using System.Threading;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil
{
    public sealed class LazyDisposable<T> : IDisposable
        where T : IDisposable
    {
        private int _hasInner;
        private int _state;

        public LazyDisposable()
        {
            Inner = default;
            _hasInner = 0;
            _state = 0;
        }

        public T? Inner { get; private set; }
        public bool CanAssign => _hasInner == 0;
        public bool IsDisposed => _state != 0;

        public void Dispose()
        {
            if ( Interlocked.Exchange( ref _state, 1 ) == 1 )
                return;

            if ( _hasInner != 0 )
                Inner!.Dispose();
        }

        public void Assign(T inner)
        {
            if ( Interlocked.Exchange( ref _hasInner, 1 ) == 1 )
                throw new LazyDisposableAssignmentException();

            Inner = inner;
            if ( _state != 0 )
                Inner.Dispose();
        }
    }
}
