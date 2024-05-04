using System;

namespace LfrlAnvil.Reactive.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid event publisher disposal.
/// </summary>
public class InvalidEventPublisherDisposalException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="InvalidEventPublisherDisposalException"/> instance.
    /// </summary>
    public InvalidEventPublisherDisposalException()
        : base( Resources.InvalidEventPublisherDisposal ) { }
}
