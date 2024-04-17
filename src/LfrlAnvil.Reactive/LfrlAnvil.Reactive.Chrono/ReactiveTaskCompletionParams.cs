using System;
using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public readonly record struct ReactiveTaskCompletionParams(
    ReactiveTaskInvocationParams Invocation,
    Duration ElapsedTime,
    Exception? Exception,
    bool IsCancelled
)
{
    [MemberNotNullWhen( true, nameof( Exception ) )]
    public bool IsFailed => Exception is not null;

    public bool IsSuccessful => ! IsFailed && ! IsCancelled;
}
