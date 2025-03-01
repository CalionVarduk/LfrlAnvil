namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Defines possible <see cref="MessageBrokerLinkedChannel"/> states.
/// </summary>
public enum MessageBrokerLinkedChannelState : byte
{
    /// <summary>
    /// Specifies that the channel is linked to the client and allows to publish messages.
    /// </summary>
    Linked = 0,

    /// <summary>
    /// Specifies that the channel has been unlinked from the client.
    /// </summary>
    Unlinked = 1
}
