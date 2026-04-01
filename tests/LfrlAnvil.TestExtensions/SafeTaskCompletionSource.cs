using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.TestExtensions;

public sealed class SafeTaskCompletionSource
{
    private readonly TaskCompletionSource _base;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly CancellationTokenRegistration _registration;
    private readonly TimeSpan _cancellationTimeout;
    private readonly int _completionCount;
    private int _isCancellationActive;
    private int _currentCompletionCount;

    public SafeTaskCompletionSource(int completionCount = 1, TimeSpan? timeout = null)
    {
        _isCancellationActive = 0;
        _currentCompletionCount = 0;
        _completionCount = Math.Max( completionCount, 1 );
        _base = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        _cancellationSource = new CancellationTokenSource();
        _cancellationTimeout = timeout ?? TimeSpan.FromSeconds( 15 );
        _registration = _cancellationSource.Token.Register( (_, ct) => _base.SetException( new OperationCanceledException( ct ) ), null );
    }

    public Task Task
    {
        get
        {
            if ( Interlocked.Exchange( ref _isCancellationActive, 1 ) == 0 )
            {
                try
                {
                    _cancellationSource.CancelAfter( _cancellationTimeout );
                }
                catch ( ObjectDisposedException )
                {
                    // NOTE: do nothing
                }
            }

            return _base.Task;
        }
    }

    public bool Complete()
    {
        if ( Interlocked.Increment( ref _currentCompletionCount ) < _completionCount )
            return false;

        _registration.Dispose();
        _cancellationSource.Dispose();
        _base.SetResult();
        return true;
    }

    public bool TryComplete()
    {
        int count;
        do
        {
            count = Volatile.Read( ref _currentCompletionCount );
            if ( count >= _completionCount )
                return false;
        }
        while ( Interlocked.CompareExchange( ref _currentCompletionCount, count + 1, count ) != count );

        if ( count + 1 != _completionCount )
            return false;

        _registration.Dispose();
        _cancellationSource.Dispose();
        _base.SetResult();
        return true;
    }
}

public sealed class SafeTaskCompletionSource<T>
{
    private readonly TaskCompletionSource<T> _base;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly CancellationTokenRegistration _registration;
    private readonly TimeSpan _cancellationTimeout;
    private readonly int _completionCount;
    private int _isCancellationActive;
    private int _currentCompletionCount;

    public SafeTaskCompletionSource(int completionCount = 1, TimeSpan? timeout = null)
    {
        _currentCompletionCount = 0;
        _completionCount = Math.Max( completionCount, 1 );
        _base = new TaskCompletionSource<T>( TaskCreationOptions.RunContinuationsAsynchronously );
        _cancellationSource = new CancellationTokenSource();
        _cancellationTimeout = timeout ?? TimeSpan.FromSeconds( 15 );
        _registration = _cancellationSource.Token.Register( (_, ct) => _base.SetException( new OperationCanceledException( ct ) ), null );
    }

    public Task<T> Task
    {
        get
        {
            if ( Interlocked.Exchange( ref _isCancellationActive, 1 ) == 0 )
            {
                try
                {
                    _cancellationSource.CancelAfter( _cancellationTimeout );
                }
                catch ( ObjectDisposedException )
                {
                    // NOTE: do nothing
                }
            }

            return _base.Task;
        }
    }

    public bool Complete(T result)
    {
        if ( Interlocked.Increment( ref _currentCompletionCount ) < _completionCount )
            return false;

        _registration.Dispose();
        _cancellationSource.Dispose();
        _base.SetResult( result );
        return true;
    }

    public bool TryComplete(T result)
    {
        int count;
        do
        {
            count = Volatile.Read( ref _currentCompletionCount );
            if ( count >= _completionCount )
                return false;
        }
        while ( Interlocked.CompareExchange( ref _currentCompletionCount, count + 1, count ) != count );

        if ( count + 1 != _completionCount )
            return false;

        _registration.Dispose();
        _cancellationSource.Dispose();
        _base.SetResult( result );
        return true;
    }
}
