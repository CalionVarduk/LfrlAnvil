using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents parameters of reactive task invocation.
/// </summary>
/// <param name="InvocationId">Id of the invocation.</param>
/// <param name="OriginalTimestamp"><see cref="Timestamp"/> at which this invocation should have happened.</param>
/// <param name="InvocationTimestamp"><see cref="Timestamp"/> at which this invocation happened.</param>
public readonly record struct ReactiveTaskInvocationParams(long InvocationId, Timestamp OriginalTimestamp, Timestamp InvocationTimestamp);
