namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a type-erased event listener.
/// </summary>
public interface IEventListener
{
    /// <summary>
    /// Handler invoked during reaction to an event.
    /// </summary>
    /// <param name="event">Published event.</param>
    void React(object? @event);

    /// <summary>
    /// Handler invoked during owner's disposal.
    /// </summary>
    /// <param name="source"><see cref="DisposalSource"/> that caused the invocation.</param>
    void OnDispose(DisposalSource source);
}

/// <summary>
/// Represents a generic event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventListener<in TEvent> : IEventListener
{
    /// <summary>
    /// Handler invoked during reaction to an event.
    /// </summary>
    /// <param name="event">Published event.</param>
    void React(TEvent @event);
}
