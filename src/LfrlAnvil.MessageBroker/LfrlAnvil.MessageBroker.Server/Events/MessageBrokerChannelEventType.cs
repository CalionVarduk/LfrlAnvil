namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Defines available <see cref="MessageBrokerChannelEvent"/> types.
/// </summary>
public enum MessageBrokerChannelEventType : byte
{
    /// <summary>
    /// Specifies that the channel has encountered an unexpected error.
    /// </summary>
    Unexpected,

    /// <summary>
    /// Specifies that the channel instance has been created.
    /// </summary>
    Created,

    /// <summary>
    /// Specifies that the channel instance has been linked to a client.
    /// </summary>
    Linked,

    /// <summary>
    /// Specifies that the channel instance has been unlinked from a client.
    /// </summary>
    Unlinked,

    /// <summary>
    /// Specifies that the channel is about to be disposed.
    /// </summary>
    Disposing,

    /// <summary>
    /// Specifies that the channel has been disposed.
    /// </summary>
    Disposed
}
