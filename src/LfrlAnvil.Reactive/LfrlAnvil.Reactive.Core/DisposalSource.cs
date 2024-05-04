namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents an information about the source of <see cref="IEventListener"/> disposal.
/// </summary>
public enum DisposalSource : byte
{
    /// <summary>
    /// Specifies that the whole event source has been disposed.
    /// </summary>
    EventSource = 0,

    /// <summary>
    /// Specifies that only the event publisher attached to the event listener has been disposed.
    /// </summary>
    Subscriber = 1
}
