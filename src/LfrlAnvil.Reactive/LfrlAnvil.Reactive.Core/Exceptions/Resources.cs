using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Exceptions;

internal static class Resources
{
    internal const string DisposedEventSource = "Event source is disposed.";
    internal const string DisposedEventExchange = "Event exchange is disposed.";
    internal const string CurrentSynchronizationContextCannotBeNull = "Current synchronization context cannot be null.";

    internal const string InvalidEventPublisherDisposal =
        "Disposal subscriber of a publisher owned by an event exchange cannot be manually disposed.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidArgumentType(object? argument, Type expectedType)
    {
        var argumentTypeName = argument is null ? "<null>" : argument.GetType().FullName;
        return $"Expected argument of type {expectedType.FullName} but found {argumentTypeName}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EventPublisherNotFound(Type eventType)
    {
        return $"Event publisher for event type {eventType.FullName} was not found.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EventPublisherAlreadyExists(Type eventType)
    {
        return $"Event publisher for event type {eventType.FullName} already exists.";
    }
}