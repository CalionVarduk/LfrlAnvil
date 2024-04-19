namespace LfrlAnvil.Reactive.Chrono;

public enum TaskCancellationReason : byte
{
    CancellationRequested = 0,
    MaxQueueSizeLimit = 1,
    TaskDisposed = 2
}
