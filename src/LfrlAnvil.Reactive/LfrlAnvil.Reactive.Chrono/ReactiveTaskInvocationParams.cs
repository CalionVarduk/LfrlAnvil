using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public readonly record struct ReactiveTaskInvocationParams(long InvocationId, Timestamp OriginalTimestamp, Timestamp InvocationTimestamp);
