using System;

namespace LfrlAnvil.MessageBroker.Server.Exceptions;

/// <summary>
/// Represents an error encountered during channel-client linkage attempt.
/// </summary>
public class MessageBrokerRemoteClientChannelLinkException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientChannelLinkException"/> instance.
    /// </summary>
    /// <param name="client"><see cref="MessageBrokerServer"/> instance that encountered this error.</param>
    /// <param name="channel"><see cref="MessageBrokerChannel"/> instance that encountered this error.</param>
    /// <param name="message">Underlying error message.</param>
    public MessageBrokerRemoteClientChannelLinkException(MessageBrokerRemoteClient client, MessageBrokerChannel channel, string message)
        : base( message )
    {
        Client = client;
        Channel = channel;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> instance that encountered this error.
    /// </summary>
    public MessageBrokerChannel Channel { get; }
}
