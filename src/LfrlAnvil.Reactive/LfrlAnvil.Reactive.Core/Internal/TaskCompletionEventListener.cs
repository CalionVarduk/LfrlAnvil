using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class TaskCompletionEventListener<TEvent> : EventListener<TEvent>
{
    private readonly TaskCompletionSource<TEvent?> _completionSource;
    private readonly LazyDisposable<CancellationTokenRegistration> _cancellationTokenRegistration;
    private TEvent? _value;
    private bool _cancelled;

    internal TaskCompletionEventListener(
        TaskCompletionSource<TEvent?> completionSource,
        LazyDisposable<CancellationTokenRegistration> cancellationTokenRegistration)
    {
        _completionSource = completionSource;
        _cancellationTokenRegistration = cancellationTokenRegistration;
        _value = default;
        _cancelled = false;
    }

    public override void React(TEvent @event)
    {
        _value = @event;
    }

    public override void OnDispose(DisposalSource source)
    {
        _cancellationTokenRegistration.Dispose();

        var lastValue = _value;
        _value = default;

        if ( _cancelled )
        {
            _completionSource.SetCanceled();
            return;
        }

        _completionSource.SetResult( lastValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsCancelled()
    {
        _cancelled = true;
    }
}
