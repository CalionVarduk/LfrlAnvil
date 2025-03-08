using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.TestExtensions;

public sealed class SafeTaskCompletionSource
{
    private readonly TaskCompletionSource _base;
    private readonly CancellationTokenRegistration _registration;
    private readonly int _completionCount;
    private int _currentCompletionCount;

    public SafeTaskCompletionSource(int completionCount = 1, TimeSpan? timeout = null)
    {
        _currentCompletionCount = 0;
        _completionCount = Math.Max( completionCount, 1 );
        _base = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var cancellation = new CancellationTokenSource();
        _registration = cancellation.Token.Register( (_, ct) => _base.SetException( new OperationCanceledException( ct ) ), null );
        cancellation.CancelAfter( timeout ?? TimeSpan.FromSeconds( 5 ) );
    }

    public Task Task => _base.Task;

    public bool Complete()
    {
        if ( Interlocked.Increment( ref _currentCompletionCount ) < _completionCount )
            return false;

        _registration.Dispose();
        _base.SetResult();
        return true;
    }
}

public sealed class SafeTaskCompletionSource<T>
{
    private readonly TaskCompletionSource<T> _base;
    private readonly CancellationTokenRegistration _registration;
    private readonly int _completionCount;
    private int _currentCompletionCount;

    public SafeTaskCompletionSource(int completionCount = 1, TimeSpan? timeout = null)
    {
        _currentCompletionCount = 0;
        _completionCount = Math.Max( completionCount, 1 );
        _base = new TaskCompletionSource<T>( TaskCreationOptions.RunContinuationsAsynchronously );
        var cancellation = new CancellationTokenSource();
        _registration = cancellation.Token.Register( (_, ct) => _base.SetException( new OperationCanceledException( ct ) ), null );
        cancellation.CancelAfter( timeout ?? TimeSpan.FromSeconds( 5 ) );
    }

    public Task<T> Task => _base.Task;

    public bool Complete(T result)
    {
        if ( Interlocked.Increment( ref _currentCompletionCount ) < _completionCount )
            return false;

        _registration.Dispose();
        _base.SetResult( result );
        return true;
    }
}
