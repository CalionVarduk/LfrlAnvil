using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.TestExtensions;

public sealed class SafeTaskCompletionSource
{
    private readonly TaskCompletionSource _base;
    private readonly CancellationTokenRegistration _registration;

    public SafeTaskCompletionSource(TimeSpan? timeout = null)
    {
        _base = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var cancellation = new CancellationTokenSource();
        _registration = cancellation.Token.Register( (_, ct) => _base.SetException( new OperationCanceledException( ct ) ), null );
        cancellation.CancelAfter( timeout ?? TimeSpan.FromSeconds( 5 ) );
    }

    public Task Task => _base.Task;

    public void Complete()
    {
        _registration.Dispose();
        _base.SetResult();
    }
}

public sealed class SafeTaskCompletionSource<T>
{
    private readonly TaskCompletionSource<T> _base;
    private readonly CancellationTokenRegistration _registration;

    public SafeTaskCompletionSource(TimeSpan? timeout = null)
    {
        _base = new TaskCompletionSource<T>( TaskCreationOptions.RunContinuationsAsynchronously );
        var cancellation = new CancellationTokenSource();
        _registration = cancellation.Token.Register( (_, ct) => _base.SetException( new OperationCanceledException( ct ) ), null );
        cancellation.CancelAfter( timeout ?? TimeSpan.FromSeconds( 5 ) );
    }

    public Task<T> Task => _base.Task;

    public void Complete(T result)
    {
        _registration.Dispose();
        _base.SetResult( result );
    }
}
