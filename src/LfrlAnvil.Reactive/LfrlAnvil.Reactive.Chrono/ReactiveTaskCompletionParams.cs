using System;
using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents parameters of reactive task completion.
/// </summary>
/// <param name="Invocation">Invocation parameters.</param>
/// <param name="ElapsedTime">Amount of time taken for this invocation to end.</param>
/// <param name="Exception">Optional exception thrown by this invocation.</param>
/// <param name="CancellationReason">Optional cancellation reason that caused this task to end.</param>
public readonly record struct ReactiveTaskCompletionParams(
    ReactiveTaskInvocationParams Invocation,
    Duration ElapsedTime,
    Exception? Exception,
    TaskCancellationReason? CancellationReason
)
{
    /// <summary>
    /// Specifies whether or not the invocation ended with an error.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Exception ) )]
    public bool IsFailed => Exception is not null;

    /// <summary>
    /// Specifies whether or not the invocation ended successfully.
    /// </summary>
    public bool IsSuccessful => ! IsFailed && CancellationReason is null;
}
