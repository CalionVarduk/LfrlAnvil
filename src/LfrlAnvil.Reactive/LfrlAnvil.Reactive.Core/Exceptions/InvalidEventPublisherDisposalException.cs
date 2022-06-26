using System;

namespace LfrlAnvil.Reactive.Exceptions;

public class InvalidEventPublisherDisposalException : InvalidOperationException
{
    public InvalidEventPublisherDisposalException()
        : base( Resources.InvalidEventPublisherDisposal ) { }
}